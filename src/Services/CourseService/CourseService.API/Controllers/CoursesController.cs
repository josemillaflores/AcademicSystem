using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseService.Application.Commands;
using CourseService.Application.DTOs;
using CourseService.Application.Queries;

namespace CourseService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Default")]
[Produces("application/json")]
public class CoursesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CoursesController> _logger;

    public CoursesController(IMediator mediator, ILogger<CoursesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los cursos con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CourseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<CourseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] int? minCredits = null,
        [FromQuery] int? maxCredits = null)
    {
        var query = new GetAllCoursesQuery(page, pageSize, status, minCredits, maxCredits);
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
    /// Obtiene un curso por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto>> GetById(Guid id)
    {
        var query = new GetCourseQuery(id);
        var course = await _mediator.Send(query);
        
        if (course == null)
            return NotFound(new { error = $"Course with ID {id} not found" });
            
        return Ok(course);
    }

    /// <summary>
    /// Obtiene un curso por código
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(CourseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CourseDto>> GetByCode(string code)
    {
        var query = new GetCourseByCodeQuery(code);
        var course = await _mediator.Send(query);
        
        if (course == null)
            return NotFound(new { error = $"Course with code {code} not found" });
            
        return Ok(course);
    }

    /// <summary>
    /// Crea un nuevo curso
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCourseCommand command)
    {
        // Verificar si el código ya existe
        var codeExists = await _mediator.Send(new CheckCourseCodeExistsQuery(command.Code));
        if (codeExists)
            return Conflict(new { error = $"Course code {command.Code} already exists" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        _logger.LogInformation("Course created with ID: {CourseId}, Code: {Code}", result.Data, command.Code);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
    }

    /// <summary>
    /// Actualiza un curso existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCourseCommand command)
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
            
        _logger.LogInformation("Course updated: {CourseId}", id);
        return NoContent();
    }

    /// <summary>
    /// Elimina un curso
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteCourseCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
            
        _logger.LogInformation("Course deleted: {CourseId}", id);
        return NoContent();
    }

    /// <summary>
    /// Agrega un prerrequisito a un curso
    /// </summary>
    [HttpPost("{id:guid}/prerequisites")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddPrerequisite(Guid id, [FromBody] AddPrerequisiteCommand command)
    {
        if (id != command.CourseId)
            return BadRequest(new { error = "Route ID does not match course ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        return Ok(new { message = "Prerequisite added successfully", prerequisiteId = result.Data });
    }

    /// <summary>
    /// Obtiene los prerrequisitos de un curso
    /// </summary>
    [HttpGet("{id:guid}/prerequisites")]
    [ProducesResponseType(typeof(IEnumerable<PrerequisiteDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PrerequisiteDto>>> GetPrerequisites(Guid id)
    {
        var query = new GetCoursePrerequisitesQuery(id);
        var prerequisites = await _mediator.Send(query);
        return Ok(prerequisites);
    }

    /// <summary>
    /// Elimina un prerrequisito
    /// </summary>
    [HttpDelete("{id:guid}/prerequisites/{prerequisiteId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemovePrerequisite(Guid id, Guid prerequisiteId)
    {
        var command = new RemovePrerequisiteCommand(id, prerequisiteId);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
            
        return NoContent();
    }

    /// <summary>
    /// Verifica disponibilidad de cupo
    /// </summary>
    [HttpGet("{id:guid}/availability")]
    [ProducesResponseType(typeof(CourseAvailabilityDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseAvailabilityDto>> CheckAvailability(Guid id)
    {
        var query = new CheckCourseAvailabilityQuery(id);
        var availability = await _mediator.Send(query);
        return Ok(availability);
    }

    /// <summary>
    /// Incrementa el número de inscritos en un curso
    /// </summary>
    [HttpPost("{id:guid}/increment-enrollment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> IncrementEnrollment(Guid id)
    {
        var command = new IncrementCourseEnrollmentCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        return Ok(new { message = "Enrollment incremented successfully", newEnrollmentCount = result.Data });
    }

    /// <summary>
    /// Decrementa el número de inscritos en un curso
    /// </summary>
    [HttpPost("{id:guid}/decrement-enrollment")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DecrementEnrollment(Guid id)
    {
        var command = new DecrementCourseEnrollmentCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        return Ok(new { message = "Enrollment decremented successfully", newEnrollmentCount = result.Data });
    }

    /// <summary>
    /// Obtiene cursos activos disponibles
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetAvailableCourses()
    {
        var query = new GetAvailableCoursesQuery();
        var courses = await _mediator.Send(query);
        return Ok(courses);
    }

    /// <summary>
    /// Obtiene estadísticas de cursos
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(CourseStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CourseStatisticsDto>> GetStatistics()
    {
        var query = new GetCourseStatisticsQuery();
        var statistics = await _mediator.Send(query);
        return Ok(statistics);
    }
}