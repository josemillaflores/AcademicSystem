using AutoMapper;
using EnrollmentService.Application.DTOs;
using EnrollmentService.Domain.Entities;

namespace EnrollmentService.Application.Mappings;

public class EnrollmentProfile : Profile
{
    public EnrollmentProfile()
    {
        CreateMap<Enrollment, EnrollmentDto>()
            .ForMember(dest => dest.StudentId, opt => opt.MapFrom(src => src.StudentId))
            .ForMember(dest => dest.CourseId, opt => opt.MapFrom(src => src.CourseId))
            .ForMember(dest => dest.EnrollmentDate, opt => opt.MapFrom(src => src.EnrollmentDate))
            .ForMember(dest => dest.Period, opt => opt.MapFrom(src => src.Period.Name))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.RejectionReason, opt => opt.MapFrom(src => src.RejectionReason))
            .ForMember(dest => dest.Validations, opt => opt.MapFrom(src => src.Validations));
        
        CreateMap<EnrollmentValidation, EnrollmentValidationDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.IsValid, opt => opt.MapFrom(src => src.IsValid))
            .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
            .ForMember(dest => dest.ValidatedAt, opt => opt.MapFrom(src => src.ValidatedAt));
        
        CreateMap<Enrollment, CompleteEnrollmentDto>()
            .ForMember(dest => dest.Enrollment, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.ComposedAt, opt => opt.MapFrom(src => DateTime.UtcNow));
    }
}