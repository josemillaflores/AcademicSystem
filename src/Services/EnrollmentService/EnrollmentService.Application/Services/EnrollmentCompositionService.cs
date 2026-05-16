using System.Net.Http.Json;
using EnrollmentService.Application.DTOs;

namespace EnrollmentService.Application.Services;

public interface IEnrollmentCompositionService
{
    Task<EnrollmentValidationResult> ValidateEnrollmentAsync(Guid studentId, Guid courseId);
    Task<CompleteEnrollmentDto> GetCompleteEnrollmentInfoAsync(Guid enrollmentId);
    Task<ProcessEnrollmentResult> ProcessEnrollmentAsync(ProcessEnrollmentRequest request);
}

public class EnrollmentCompositionService : IEnrollmentCompositionService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IEnrollmentRepository _repository;
    private readonly ILogger<EnrollmentCompositionService> _logger;
    private readonly IMapper _mapper;

    public EnrollmentCompositionService(
        IHttpClientFactory httpClientFactory,
        IEnrollmentRepository repository,
        ILogger<EnrollmentCompositionService> logger,
        IMapper mapper)
    {
        _httpClientFactory = httpClientFactory;
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<EnrollmentValidationResult> ValidateEnrollmentAsync(Guid studentId, Guid courseId)
    {
        _logger.LogInformation("Starting enrollment validation for Student {StudentId}, Course {CourseId}", studentId, courseId);

        var result = new EnrollmentValidationResult();

        // Se ejecutan de manera secuencial para evitar condiciones de carrera modificando 'result.Errors'
        // y para garantizar que 'result.StudentInfo' esté poblado antes de validar los prerrequisitos del curso.
        await ValidateStudentAsync(studentId, result);
        await ValidateCourseAsync(courseId, result);

        result.IsValid = result.StudentIsValid && result.CourseIsValid && 
                         result.PrerequisitesAreMet;

        _logger.LogInformation("Validation completed. IsValid: {IsValid}", result.IsValid);
        return result;
    }

    private async Task ValidateStudentAsync(Guid studentId, EnrollmentValidationResult result)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("StudentService");
            var student = await client.GetFromJsonAsync<StudentInfoDto>($"/api/v1/students/{studentId}");
            
            if (student == null)
            {
                result.StudentIsValid = false;
                result.Errors.Add($"Student {studentId} not found");
                return;
            }

            result.StudentIsValid = student.Status == "Active";
            result.StudentInfo = student;
            
            if (!result.StudentIsValid)
                result.Errors.Add($"Student is not active. Current status: {student.Status}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating student {StudentId}", studentId);
            result.StudentIsValid = false;
            result.Errors.Add($"Error validating student: {ex.Message}");
        }
    }

    private async Task ValidateCourseAsync(Guid courseId, EnrollmentValidationResult result)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("CourseService");
            var course = await client.GetFromJsonAsync<CourseInfoDto>($"/api/v1/courses/{courseId}");
            
            if (course == null)
            {
                result.CourseIsValid = false;
                result.Errors.Add($"Course {courseId} not found");
                return;
            }

            result.CourseIsValid = course.Status == "Active" && course.HasAvailableSlots;
            result.CourseInfo = course;
            
            if (!result.CourseIsValid)
            {
                if (course.Status != "Active")
                    result.Errors.Add($"Course is not active. Status: {course.Status}");
                if (!course.HasAvailableSlots)
                    result.Errors.Add($"Course has no available slots. Current enrollment: {course.CurrentEnrollment}/{course.MaxCapacity}");
            }

            if (result.StudentInfo != null && course.Prerequisites.Any())
            {
                result.PrerequisitesAreMet = await ValidatePrerequisitesAsync(
                    result.StudentInfo.CompletedCourses, 
                    course.Prerequisites);
                
                if (!result.PrerequisitesAreMet)
                    result.Errors.Add("Student does not meet course prerequisites");
            }
            else
            {
                // Si no hay prerrequisitos, la regla se cumple automáticamente
                result.PrerequisitesAreMet = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating course {CourseId}", courseId);
            result.CourseIsValid = false;
            result.Errors.Add($"Error validating course: {ex.Message}");
        }
    }

    private async Task<bool> ValidatePrerequisitesAsync(
        List<CompletedCourseDto> completedCourses, 
        List<PrerequisiteInfoDto> prerequisites)
    {
        if (!prerequisites.Any())
            return true;

        var completedCourseIds = completedCourses.Select(c => c.CourseId).ToHashSet();
        
        foreach (var prerequisite in prerequisites)
        {
            if (prerequisite.IsMandatory && !completedCourseIds.Contains(prerequisite.RequiredCourseId))
                return false;
        }
        
        return await Task.FromResult(true);
    }

    public async Task<CompleteEnrollmentDto> GetCompleteEnrollmentInfoAsync(Guid enrollmentId)
    {
        _logger.LogInformation("Composing complete enrollment info for {EnrollmentId}", enrollmentId);
        
        var enrollment = await _repository.GetByIdAsync(enrollmentId);
        
        if (enrollment == null)
            return new CompleteEnrollmentDto();
        
        var result = new CompleteEnrollmentDto
        {
            Enrollment = _mapper.Map<EnrollmentDto>(enrollment),
            ComposedAt = DateTime.UtcNow
        };
        
        // Componer información del estudiante
        var studentClient = _httpClientFactory.CreateClient("StudentService");
        result.Student = await studentClient.GetFromJsonAsync<StudentInfoDto>(
            $"/api/v1/students/{enrollment.StudentId}");
        
        // Componer información del curso
        var courseClient = _httpClientFactory.CreateClient("CourseService");
        result.Course = await courseClient.GetFromJsonAsync<CourseInfoDto>(
            $"/api/v1/courses/{enrollment.CourseId}");
        
        return result;
    }

    public async Task<ProcessEnrollmentResult> ProcessEnrollmentAsync(ProcessEnrollmentRequest request)
    {
        _logger.LogInformation("Processing enrollment for Student {StudentId}, Course {CourseId}", 
            request.StudentId, request.CourseId);
        
        var result = new ProcessEnrollmentResult();
        
        // 1. Validar matrícula
        var validation = await ValidateEnrollmentAsync(request.StudentId, request.CourseId);
        if (!validation.IsValid)
        {
            result.Success = false;
            result.Errors = validation.Errors;
            return result;
        }
        
        // 2. Crear matrícula
        var enrollment = new Enrollment(request.StudentId, request.CourseId, GetCurrentPeriod());
        await _repository.AddAsync(enrollment);
        await _repository.SaveChangesAsync();
        
        result.EnrollmentId = enrollment.Id;
        result.Success = true;
        result.Message = "Enrollment processed successfully";
        
        _logger.LogInformation("Enrollment {EnrollmentId} processed successfully", result.EnrollmentId);
        return result;
    }
    
    private string GetCurrentPeriod()
    {
        var now = DateTime.UtcNow;
        var year = now.Year;
        var semester = now.Month <= 7 ? "1" : "2";
        return $"{year}-{semester}";
    }
}