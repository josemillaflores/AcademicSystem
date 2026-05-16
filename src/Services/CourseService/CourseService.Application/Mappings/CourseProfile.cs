using AutoMapper;
using CourseService.Application.DTOs;
using CourseService.Domain.Entities;

namespace CourseService.Application.Mappings;

public class CourseProfile : Profile
{
    public CourseProfile()
    {
        CreateMap<Course, CourseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.HasAvailableSlots, opt => opt.MapFrom(src => src.HasAvailableSlots()))
            .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.MaxCapacity - src.CurrentEnrollment))
            .ForMember(dest => dest.Prerequisites, opt => opt.MapFrom(src => src.Prerequisites))
            .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src.Schedule));
        
        CreateMap<Prerequisite, PrerequisiteDto>()
            .ForMember(dest => dest.RequiredCourseId, opt => opt.MapFrom(src => src.RequiredCourseId))
            .ForMember(dest => dest.RequiredCourseName, opt => opt.MapFrom(src => src.RequiredCourseName))
            .ForMember(dest => dest.RequiredCourseCode, opt => opt.MapFrom(src => src.RequiredCourseCode))
            .ForMember(dest => dest.IsMandatory, opt => opt.MapFrom(src => src.IsMandatory));
        
        CreateMap<Course, CourseAvailabilityDto>()
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.CourseCode, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.CourseName, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.MaxCapacity, opt => opt.MapFrom(src => src.MaxCapacity))
            .ForMember(dest => dest.CurrentEnrollment, opt => opt.MapFrom(src => src.CurrentEnrollment))
            .ForMember(dest => dest.AvailableSlots, opt => opt.MapFrom(src => src.MaxCapacity - src.CurrentEnrollment))
            .ForMember(dest => dest.EnrollmentPercentage, opt => opt.MapFrom(src => (double)src.CurrentEnrollment / src.MaxCapacity * 100))
            .ForMember(dest => dest.HasAvailability, opt => opt.MapFrom(src => src.HasAvailableSlots()))
            .ForMember(dest => dest.WaitlistStatus, opt => opt.MapFrom(src => src.CurrentEnrollment >= src.MaxCapacity ? "Active" : "None"));
        
        CreateMap<Schedule, ScheduleDto>();
    }
}