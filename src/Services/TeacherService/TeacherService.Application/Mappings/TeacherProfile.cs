using AutoMapper;
using TeacherService.Application.DTOs;
using TeacherService.Domain.Entities;

namespace TeacherService.Application.Mappings;

public class TeacherProfile : Profile
{
    public TeacherProfile()
    {
        CreateMap<Teacher, TeacherDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name.FullName))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Name.LastName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.TeacherNumber, opt => opt.MapFrom(src => src.TeacherNumber.Value))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.YearsOfService, opt => opt.MapFrom(src => DateTime.UtcNow.Year - src.HireDate.Year))
            .ForMember(dest => dest.Specialties, opt => opt.MapFrom(src => src.Specialties))
            .ForMember(dest => dest.CurrentCoursesCount, opt => opt.MapFrom(src => src.CourseAssignments.Count(a => a.IsActive)))
            .ForMember(dest => dest.CurrentHours, opt => opt.MapFrom(src => src.AcademicLoad.CurrentHours))
            .ForMember(dest => dest.MaxHoursPerWeek, opt => opt.MapFrom(src => src.AcademicLoad.MaxHoursPerWeek));
        
        CreateMap<Specialty, SpecialtyDto>();
        
        CreateMap<CourseAssignment, TeacherCourseDto>()
            .ForMember(dest => dest.AssignmentId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
            .ForMember(dest => dest.Credits, opt => opt.MapFrom(src => src.Credits))
            .ForMember(dest => dest.HoursPerWeek, opt => opt.MapFrom(src => src.HoursPerWeek))
            .ForMember(dest => dest.AssignmentDate, opt => opt.MapFrom(src => src.AssignmentDate))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
        
        CreateMap<Teacher, AcademicLoadDto>()
            .ForMember(dest => dest.MaxHoursPerWeek, opt => opt.MapFrom(src => src.AcademicLoad.MaxHoursPerWeek))
            .ForMember(dest => dest.CurrentHours, opt => opt.MapFrom(src => src.AcademicLoad.CurrentHours))
            .ForMember(dest => dest.RemainingHours, opt => opt.MapFrom(src => src.AcademicLoad.RemainingHours))
            .ForMember(dest => dest.UtilizationPercentage, opt => opt.MapFrom(src => (double)src.AcademicLoad.CurrentHours / src.AcademicLoad.MaxHoursPerWeek * 100))
            .ForMember(dest => dest.AssignedCourses, opt => opt.MapFrom(src => src.CourseAssignments.Where(a => a.IsActive)));
        
        CreateMap<CourseAssignment, AssignedCourseLoadDto>()
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.CourseName))
            .ForMember(dest => dest.HoursPerWeek, opt => opt.MapFrom(src => src.HoursPerWeek))
            .ForMember(dest => dest.StudentsCount, opt => opt.MapFrom(src => src.StudentsCount));
    }
}