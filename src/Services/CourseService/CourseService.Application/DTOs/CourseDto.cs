namespace CourseService.Application.DTOs;

/// <summary>
/// DTO para transferir información del curso
/// </summary>
public record CourseDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    int Credits,
    int TotalHours,
    int MaxCapacity,
    int CurrentEnrollment,
    string Status,
    bool HasAvailableSlots,
    int AvailableSlots,
    List<PrerequisiteDto> Prerequisites,
    ScheduleDto? Schedule
)
{
    public CourseDto() : this(Guid.Empty, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, string.Empty, false, 0, new List<PrerequisiteDto>(), null) { }
}

/// <summary>
/// DTO para horario del curso
/// </summary>
public record ScheduleDto(
    string Day,
    TimeSpan StartTime,
    TimeSpan EndTime,
    string Classroom
)
{
    public ScheduleDto() : this(string.Empty, TimeSpan.Zero, TimeSpan.Zero, string.Empty) { }
}

/// <summary>
/// DTO para creación de curso
/// </summary>
public record CreateCourseDto(
    string Code,
    string Name,
    string Description,
    int Credits,
    int TotalHours,
    int MaxCapacity
);

/// <summary>
/// DTO para actualización de curso
/// </summary>
public record UpdateCourseDto(
    string Name,
    string Description,
    int Credits,
    int MaxCapacity
);

/// <summary>
/// DTO para resultados paginados
/// </summary>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int Page,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}