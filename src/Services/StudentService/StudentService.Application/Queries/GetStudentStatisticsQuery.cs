using MediatR;
using StudentService.Application.DTOs;
using StudentService.Domain.Enums;
using StudentService.Domain.Interfaces;

namespace StudentService.Application.Queries;

public record GetStudentStatisticsQuery() : IRequest<StudentStatisticsDto>;

public class GetStudentStatisticsQueryHandler : IRequestHandler<GetStudentStatisticsQuery, StudentStatisticsDto>
{
    private readonly IStudentRepository _repository;

    public GetStudentStatisticsQueryHandler(IStudentRepository repository)
    {
        _repository = repository;
    }

    public async Task<StudentStatisticsDto> Handle(GetStudentStatisticsQuery request, CancellationToken cancellationToken)
    {
        var students = await _repository.GetAllAsync(cancellationToken);
        var studentList = students.ToList();
        
        var activeStudents = studentList.Count(s => s.Status == EnrollmentStatus.Active);
        var graduatedStudents = studentList.Count(s => s.Status == EnrollmentStatus.Graduated);
        var suspendedStudents = studentList.Count(s => s.Status == EnrollmentStatus.Suspended);
        
        return new StudentStatisticsDto(
            TotalStudents: studentList.Count,
            ActiveStudents: activeStudents,
            InactiveStudents: studentList.Count(s => s.Status == EnrollmentStatus.Inactive),
            GraduatedStudents: graduatedStudents,
            SuspendedStudents: suspendedStudents,
            AverageAge: 0,
            NewStudentsThisMonth: studentList.Count(s => s.CreatedAt.Month == DateTime.UtcNow.Month),
            AverageGPA: studentList.Average(s => s.AcademicRecord.GPA),
            StudentsByProgram: new Dictionary<string, int>(),
            StudentsByStatus: new Dictionary<string, int>
            {
                ["Active"] = activeStudents,
                ["Graduated"] = graduatedStudents,
                ["Suspended"] = suspendedStudents
            }
        );
    }
}