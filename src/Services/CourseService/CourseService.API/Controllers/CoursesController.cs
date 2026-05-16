using MediatR;
using Microsoft.AspNetCore.Mvc;
using CourseService.Application.Commands;
using CourseService.Application.Queries;
using CourseService.Application.DTOs;

namespace CourseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// Obtiene todos los cursos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetAll()
    {
        var query = new GetAllCoursesQuery();
        var courses = await _mediator.Send(query);
        return Ok(courses);
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
            return NotFound($"Curso con ID {id} no encontrado");
            
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
            return NotFound($"Curso con código {code} no encontrado");
            
        return Ok(course);
    }

    /// <summary>
    /// Crea un nuevo curso
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateCourseCommand command)
    {
        try
        {
            var courseId = await _mediator.Send(command);
            _logger.LogInformation("Curso creado con ID: {CourseId}", courseId);
            return CreatedAtAction(nameof(GetById), new { id = courseId }, courseId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear curso");
            return BadRequest(new { error = ex.Message });
        }
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
            return BadRequest("El ID de la ruta no coincide con el ID del comando");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);
            
        _logger.LogInformation("Curso actualizado: {CourseId}", id);
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
            return NotFound(result.Error);
            
        _logger.LogInformation("Curso eliminado: {CourseId}", id);
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
            return BadRequest("El ID del curso no coincide");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        return Ok(new { message = "Prerrequisito agregado exitosamente" });
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
            return BadRequest(result.Error);
            
        return Ok(new { message = "Inscripción incrementada exitosamente" });
    }

    /// <summary>
    /// Obtiene cursos por rango de créditos
    /// </summary>
    [HttpGet("by-credits")]
    [ProducesResponseType(typeof(IEnumerable<CourseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CourseDto>>> GetByCreditsRange([FromQuery] int minCredits, [FromQuery] int maxCredits)
    {
        var query = new GetCoursesByCreditsRangeQuery(minCredits, maxCredits);
        var courses = await _mediator.Send(query);
        return Ok(courses);
    }
}