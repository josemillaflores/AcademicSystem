namespace TeacherService.Application.DTOs;

/// <summary>
/// DTO para transferir información del docente
/// </summary>
public record TeacherDto(
    Guid Id,
    string FullName,
    string FirstName,
    string LastName,
    string Email,
    string TeacherNumber,
    DateTime HireDate,
    string Status,
    int YearsOfService,
    List<SpecialtyDto> Specialties,
    int CurrentCoursesCount,
    int CurrentHours,
    int MaxHoursPerWeek
);

/// <summary>
/// DTO para creación de docente
/// </summary>
public record CreateTeacherDto(
    string FirstName,
    string LastName,
    string Email,
    DateTime HireDate
);

/// <summary>
/// DTO para actualización de docente
/// </summary>
public record UpdateTeacherDto(
    string FirstName,
    string LastName,
    string Email
);