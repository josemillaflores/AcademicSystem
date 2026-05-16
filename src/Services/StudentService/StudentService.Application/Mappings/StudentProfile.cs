using AutoMapper;
using StudentService.Application.DTOs;
using StudentService.Domain.Entities;
using StudentService.Domain.ValueObjects;

namespace StudentService.Application.Mappings;

public class StudentProfile : Profile
{
    public StudentProfile()
    {
        CreateMap<Student, StudentDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name.FullName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Name.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.StudentNumber, opt => opt.MapFrom(src => src.StudentNumber.Value))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.TotalCredits, opt => opt.MapFrom(src => src.AcademicRecord.TotalCredits))
            .ForMember(dest => dest.GPA, opt => opt.MapFrom(src => src.AcademicRecord.GPA))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.ContactInfo.Phone))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt));
        
        CreateMap<Student, AcademicRecordDto>()
            .ForMember(dest => dest.GPA, opt => opt.MapFrom(src => src.AcademicRecord.GPA))
            .ForMember(dest => dest.TotalCredits, opt => opt.MapFrom(src => src.AcademicRecord.TotalCredits))
            .ForMember(dest => dest.CompletedCourses, opt => opt.MapFrom(src => src.AcademicRecord.CompletedCourses))
            .ForMember(dest => dest.CurrentSemester, opt => opt.MapFrom(src => CalculateSemester(src.EnrollmentDate)))
            .ForMember(dest => dest.AcademicStatus, opt => opt.MapFrom(src => GetAcademicStatus(src)));
        
        CreateMap<CourseEnrollment, StudentCourseDto>()
            .ForMember(dest => dest.EnrollmentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
            .ForMember(dest => dest.Credits, opt => opt.MapFrom(src => src.Credits))
            .ForMember(dest => dest.EnrollmentDate, opt => opt.MapFrom(src => src.EnrollmentDate))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => src.Grade))
            .ForMember(dest => dest.GradeLetter, opt => opt.MapFrom(src => GetGradeLetter(src.Grade)));
        
        CreateMap<CompletedCourse, CompletedCourseDto>()
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
            .ForMember(dest => dest.Credits, opt => opt.MapFrom(src => src.Credits))
            .ForMember(dest => dest.Grade, opt => opt.MapFrom(src => src.Grade))
            .ForMember(dest => dest.GradeLetter, opt => opt.MapFrom(src => GetGradeLetter(src.Grade)))
            .ForMember(dest => dest.CompletionDate, opt => opt.MapFrom(src => src.CompletionDate))
            .ForMember(dest => dest.Semester, opt => opt.MapFrom(src => src.Semester));
    }
    
    private static int CalculateSemester(DateTime enrollmentDate)
    {
        var months = (DateTime.UtcNow.Year - enrollmentDate.Year) * 12 + (DateTime.UtcNow.Month - enrollmentDate.Month);
        return (months / 6) + 1;
    }
    
    private static string GetAcademicStatus(Student student)
    {
        if (student.AcademicRecord.GPA >= 3.0) return "Good Standing";
        if (student.AcademicRecord.GPA >= 2.0) return "Probation";
        return "Academic Warning";
    }
    
    private static string GetGradeLetter(double? grade)
    {
        if (!grade.HasValue) return "N/A";
        return grade.Value switch
        {
            >= 90 => "A",
            >= 80 => "B",
            >= 70 => "C",
            >= 60 => "D",
            _ => "F"
        };
    }
}