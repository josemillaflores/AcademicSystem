using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeacherService.Application.Commands;
using TeacherService.Application.DTOs;
using TeacherService.Application.Queries;

namespace TeacherService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Default")]
[Produces("application/json")]
public class TeachersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TeachersController> _logger;

    public TeachersController(IMediator mediator, ILogger<TeachersController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los docentes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TeacherDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TeacherDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllTeachersQuery(page, pageSize);
        var result = await _mediator.Send(query);
        
        Response.Headers.Append("X-Pagination", System.Text.Json.JsonSerializer.Serialize(new
        {
            result.Page,
            result.PageSize,
            result.TotalCount,
            result.TotalPages
        }));
        
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un docente por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeacherDto>> GetById(Guid id)
    {
        var query = new GetTeacherQuery(id);
        var teacher = await _mediator.Send(query);
        
        if (teacher == null)
            return NotFound(new { error = $"Teacher with ID {id} not found" });
            
        return Ok(teacher);
    }

    /// <summary>
    /// Obtiene un docente por número de empleado
    /// </summary>
    [HttpGet("by-number/{teacherNumber}")]
    [ProducesResponseType(typeof(TeacherDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TeacherDto>> GetByTeacherNumber(string teacherNumber)
    {
        var query = new GetTeacherByNumberQuery(teacherNumber);
        var teacher = await _mediator.Send(query);
        
        if (teacher == null)
            return NotFound(new { error = $"Teacher with number {teacherNumber} not found" });
            
        return Ok(teacher);
    }

    /// <summary>
    /// Crea un nuevo docente
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateTeacherCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        _logger.LogInformation("Teacher created with ID: {TeacherId}", result.Data);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
    }

    /// <summary>
    /// Actualiza un docente existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeacherCommand command)
    {
        if (id != command.Id)
            return BadRequest(new { error = "Route ID does not match command ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.Error.Contains("not found"))
                return NotFound(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }
            
        _logger.LogInformation("Teacher updated: {TeacherId}", id);
        return NoContent();
    }

    /// <summary>
    /// Elimina un docente
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteTeacherCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
            
        _logger.LogInformation("Teacher deleted: {TeacherId}", id);
        return NoContent();
    }

    /// <summary>
    /// Agrega una especialidad a un docente
    /// </summary>
    [HttpPost("{id:guid}/specialties")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddSpecialty(Guid id, [FromBody] AddTeacherSpecialtyCommand command)
    {
        if (id != command.TeacherId)
            return BadRequest(new { error = "Route ID does not match teacher ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        return Ok(new { message = "Specialty added successfully", specialtyId = result.Data });
    }

    /// <summary>
    /// Obtiene las especialidades de un docente
    /// </summary>
    [HttpGet("{id:guid}/specialties")]
    [ProducesResponseType(typeof(IEnumerable<SpecialtyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SpecialtyDto>>> GetSpecialties(Guid id)
    {
        var query = new GetTeacherSpecialtiesQuery(id);
        var specialties = await _mediator.Send(query);
        return Ok(specialties);
    }

    /// <summary>
    /// Elimina una especialidad de un docente
    /// </summary>
    [HttpDelete("{id:guid}/specialties/{specialtyId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSpecialty(Guid id, Guid specialtyId)
    {
        var command = new RemoveTeacherSpecialtyCommand(id, specialtyId);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
            
        return NoContent();
    }

    /// <summary>
    /// Asigna un curso a un docente
    /// </summary>
    [HttpPost("{id:guid}/assign-course")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignCourse(Guid id, [FromBody] AssignCourseToTeacherCommand command)
    {
        if (id != command.TeacherId)
            return BadRequest(new { error = "Route ID does not match teacher ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        return Ok(new { message = "Course assigned successfully" });
    }

    /// <summary>
    /// Obtiene los cursos asignados a un docente
    /// </summary>
    [HttpGet("{id:guid}/courses")]
    [ProducesResponseType(typeof(IEnumerable<TeacherCourseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TeacherCourseDto>>> GetAssignedCourses(Guid id)
    {
        var query = new GetTeacherCoursesQuery(id);
        var courses = await _mediator.Send(query);
        return Ok(courses);
    }

    /// <summary>
    /// Obtiene la carga académica de un docente
    /// </summary>
    [HttpGet("{id:guid}/academic-load")]
    [ProducesResponseType(typeof(AcademicLoadDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AcademicLoadDto>> GetAcademicLoad(Guid id)
    {
        var query = new GetTeacherAcademicLoadQuery(id);
        var academicLoad = await _mediator.Send(query);
        return Ok(academicLoad);
    }

    /// <summary>
    /// Obtiene estadísticas de docentes
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(TeacherStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TeacherStatisticsDto>> GetStatistics()
    {
        var query = new GetTeacherStatisticsQuery();
        var statistics = await _mediator.Send(query);
        return Ok(statistics);
    }
}