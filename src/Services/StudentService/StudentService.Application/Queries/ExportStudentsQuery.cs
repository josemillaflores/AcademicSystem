using AutoMapper;
using MediatR;
using StudentService.Domain.Interfaces;
using System.Text;

namespace StudentService.Application.Queries;

public record ExportStudentsQuery() : IRequest<string>;

public class ExportStudentsQueryHandler : IRequestHandler<ExportStudentsQuery, string>
{
    private readonly IStudentRepository _repository;
    private readonly IMapper _mapper;

    public ExportStudentsQueryHandler(IStudentRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<string> Handle(ExportStudentsQuery request, CancellationToken cancellationToken)
    {
        var students = await _repository.GetAllAsync(cancellationToken);
        var studentList = students.ToList();
        
        var csv = new StringBuilder();
        csv.AppendLine("ID,StudentNumber,FullName,Email,EnrollmentDate,Status,TotalCredits,GPA");
        
        foreach (var student in studentList)
        {
            csv.AppendLine($"{student.Id},{student.StudentNumber.Value},{student.Name.FullName},{student.Email.Value},{student.EnrollmentDate:yyyy-MM-dd},{student.Status},{student.AcademicRecord.TotalCredits},{student.AcademicRecord.GPA}");
        }
        
        return csv.ToString();
    }
}