using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeacherService.Application.Commands;
using TeacherService.Application.Queries;
using TeacherService.Application.DTOs;

namespace TeacherService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    public async Task<ActionResult<IEnumerable<TeacherDto>>> GetAll()
    {
        var query = new GetAllTeachersQuery();
        var teachers = await _mediator.Send(query);
        return Ok(teachers);
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
            return NotFound($"Docente con ID {id} no encontrado");
            
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
        try
        {
            var teacherId = await _mediator.Send(command);
            _logger.LogInformation("Docente creado con ID: {TeacherId}", teacherId);
            return CreatedAtAction(nameof(GetById), new { id = teacherId }, teacherId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear docente");
            return BadRequest(new { error = ex.Message });
        }
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
            return BadRequest("El ID de la ruta no coincide con el ID del comando");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);
            
        _logger.LogInformation("Docente actualizado: {TeacherId}", id);
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
            return NotFound(result.Error);
            
        _logger.LogInformation("Docente eliminado: {TeacherId}", id);
        return NoContent();
    }

    /// <summary>
    /// Asigna una especialidad a un docente
    /// </summary>
    [HttpPost("{id:guid}/specialties")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddSpecialty(Guid id, [FromBody] AddTeacherSpecialtyCommand command)
    {
        if (id != command.TeacherId)
            return BadRequest("El ID del docente no coincide");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        return Ok(new { message = "Especialidad agregada exitosamente" });
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
    /// Asigna un curso a un docente
    /// </summary>
    [HttpPost("{id:guid}/assign-course")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignCourse(Guid id, [FromBody] AssignCourseToTeacherCommand command)
    {
        if (id != command.TeacherId)
            return BadRequest("El ID del docente no coincide");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        return Ok(new { message = "Curso asignado exitosamente" });
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
}