namespace StudentService.Application.DTOs;

public record StudentDto(
    Guid Id,
    string FullName,
    string Email,
    string StudentNumber,
    DateTime EnrollmentDate,
    string Status
);