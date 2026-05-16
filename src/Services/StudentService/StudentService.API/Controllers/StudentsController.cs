using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentService.Application.Commands;
using StudentService.Application.DTOs;
using StudentService.Application.Queries;

namespace StudentService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Default")]
[Produces("application/json")]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IMediator mediator, ILogger<StudentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los estudiantes con paginación
    /// </summary>
    /// <param name="page">Número de página (default: 1)</param>
    /// <param name="pageSize">Tamaño de página (default: 10, max: 100)</param>
    /// <param name="status">Filtrar por estado (opcional)</param>
    /// <returns>Lista paginada de estudiantes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<StudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<StudentDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var query = new GetAllStudentsQuery(page, pageSize, status);
        var result = await _mediator.Send(query);
        
        Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
        {
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages,
            result.HasPreviousPage,
            result.HasNextPage
        }));
        
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un estudiante por ID
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetById(Guid id)
    {
        var query = new GetStudentQuery(id);
        var student = await _mediator.Send(query);
        
        if (student == null)
            return NotFound(new { error = $"Student with ID {id} not found" });
            
        return Ok(student);
    }

    /// <summary>
    /// Obtiene un estudiante por número de estudiante
    /// </summary>
    /// <param name="studentNumber">Número de estudiante (ej: STU12345678)</param>
    [HttpGet("by-number/{studentNumber}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetByStudentNumber(string studentNumber)
    {
        var query = new GetStudentByNumberQuery(studentNumber);
        var student = await _mediator.Send(query);
        
        if (student == null)
            return NotFound(new { error = $"Student with number {studentNumber} not found" });
            
        return Ok(student);
    }

    /// <summary>
    /// Obtiene un estudiante por email
    /// </summary>
    /// <param name="email">Email del estudiante</param>
    [HttpGet("by-email/{email}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetByEmail(string email)
    {
        var query = new GetStudentByEmailQuery(email);
        var student = await _mediator.Send(query);
        
        if (student == null)
            return NotFound(new { error = $"Student with email {email} not found" });
            
        return Ok(student);
    }

    /// <summary>
    /// Crea un nuevo estudiante
    /// </summary>
    /// <param name="command">Datos del estudiante</param>
    [HttpPost]
    [ProducesResponseType(typeof(CreateStudentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CreateStudentResponse>> Create([FromBody] CreateStudentCommand command)
    {
        // Verificar si el email ya existe
        var emailExists = await _mediator.Send(new CheckEmailExistsQuery(command.Email));
        if (emailExists)
            return Conflict(new { error = "Email already exists" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new ValidationProblemDetails
            {
                Title = "Validation Error",
                Detail = result.Error,
                Status = StatusCodes.Status400BadRequest
            });
        
        _logger.LogInformation("Student created with ID: {StudentId}", result.Data);
        
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, 
            new CreateStudentResponse { Id = result.Data, StudentNumber = result.StudentNumber });
    }

    /// <summary>
    /// Actualiza un estudiante existente
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <param name="command">Datos a actualizar</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Route ID does not match command ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.Error.Contains("not found"))
                return NotFound(new { error = result.Error });
            if (result.Error.Contains("email"))
                return Conflict(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }
            
        _logger.LogInformation("Student updated: {StudentId}", id);
        return NoContent();
    }

    /// <summary>
    /// Elimina (soft delete) un estudiante
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteStudentCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
            
        _logger.LogInformation("Student deleted (soft): {StudentId}", id);
        return NoContent();
    }

    /// <summary>
    /// Inscribe un estudiante en un curso
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    /// <param name="command">Datos de inscripción</param>
    [HttpPost("{id:guid}/enrollments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnrollInCourse(Guid id, [FromBody] EnrollStudentCommand command)
    {
        if (id != command.StudentId)
            return BadRequest(new { error = "Route ID does not match student ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.Error.Contains("not found"))
                return NotFound(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }
            
        return Ok(new { message = "Student enrolled successfully", enrollmentId = result.Data });
    }

    /// <summary>
    /// Obtiene el historial académico del estudiante
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    [HttpGet("{id:guid}/academic-record")]
    [ProducesResponseType(typeof(AcademicRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademicRecordDto>> GetAcademicRecord(Guid id)
    {
        var query = new GetStudentAcademicRecordQuery(id);
        var record = await _mediator.Send(query);
        
        if (record == null)
            return NotFound(new { error = $"Academic record for student {id} not found" });
            
        return Ok(record);
    }

    /// <summary>
    /// Obtiene los cursos en los que está inscrito el estudiante
    /// </summary>
    /// <param name="id">ID del estudiante</param>
    [HttpGet("{id:guid}/courses")]
    [ProducesResponseType(typeof(IEnumerable<StudentCourseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentCourseDto>>> GetStudentCourses(Guid id)
    {
        var query = new GetStudentCoursesQuery(id);
        var courses = await _mediator.Send(query);
        return Ok(courses);
    }

    /// <summary>
    /// Obtiene estadísticas de estudiantes
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(StudentStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentStatisticsDto>> GetStatistics()
    {
        var query = new GetStudentStatisticsQuery();
        var statistics = await _mediator.Send(query);
        return Ok(statistics);
    }

    /// <summary>
    /// Busca estudiantes por nombre
    /// </summary>
    /// <param name="searchTerm">Término de búsqueda</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentDto>>> Search([FromQuery] string searchTerm)
    {
        var query = new SearchStudentsQuery(searchTerm);
        var students = await _mediator.Send(query);
        return Ok(students);
    }

    /// <summary>
    /// Exporta estudiantes a CSV
    /// </summary>
    [HttpGet("export/csv")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToCsv()
    {
        var query = new ExportStudentsQuery();
        var csvData = await _mediator.Send(query);
        
        return File(System.Text.Encoding.UTF8.GetBytes(csvData), 
            "text/csv", 
            $"students_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
    }
}

public record CreateStudentResponse(Guid Id, string StudentNumber);