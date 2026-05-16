namespace TeacherService.Application.DTOs;

/// <summary>
/// DTO para especialidad del docente
/// </summary>
public record SpecialtyDto(
    Guid Id,
    string Name,
    string Description
);

/// <summary>
/// DTO para creación de especialidad
/// </summary>
public record CreateSpecialtyDto(
    string Name,
    string Description
);