using MediatR;
using Microsoft.AspNetCore.Mvc;
using StudentService.Application.Commands;
using StudentService.Application.Queries;
using StudentService.Application.DTOs;

namespace StudentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class StudentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StudentsController> _logger;

    public StudentsController(IMediator mediator, ILogger<StudentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los estudiantes
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<StudentDto>>> GetAll()
    {
        var query = new GetAllStudentsQuery();
        var students = await _mediator.Send(query);
        return Ok(students);
    }

    /// <summary>
    /// Obtiene un estudiante por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetById(Guid id)
    {
        var query = new GetStudentQuery(id);
        var student = await _mediator.Send(query);
        
        if (student == null)
            return NotFound($"Estudiante con ID {id} no encontrado");
            
        return Ok(student);
    }

    /// <summary>
    /// Obtiene un estudiante por número de estudiante
    /// </summary>
    [HttpGet("by-number/{studentNumber}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetByStudentNumber(string studentNumber)
    {
        var query = new GetStudentByNumberQuery(studentNumber);
        var student = await _mediator.Send(query);
        
        if (student == null)
            return NotFound($"Estudiante con número {studentNumber} no encontrado");
            
        return Ok(student);
    }

    /// <summary>
    /// Crea un nuevo estudiante
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateStudentCommand command)
    {
        try
        {
            var studentId = await _mediator.Send(command);
            _logger.LogInformation("Estudiante creado con ID: {StudentId}", studentId);
            return CreatedAtAction(nameof(GetById), new { id = studentId }, studentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear estudiante");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Actualiza un estudiante existente
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStudentCommand command)
    {
        if (id != command.Id)
            return BadRequest("El ID de la ruta no coincide con el ID del comando");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);
            
        _logger.LogInformation("Estudiante actualizado: {StudentId}", id);
        return NoContent();
    }

    /// <summary>
    /// Elimina un estudiante
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteStudentCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(result.Error);
            
        _logger.LogInformation("Estudiante eliminado: {StudentId}", id);
        return NoContent();
    }

    /// <summary>
    /// Inscribe un estudiante en un curso
    /// </summary>
    [HttpPost("{id:guid}/enroll")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> EnrollInCourse(Guid id, [FromBody] EnrollStudentCommand command)
    {
        if (id != command.StudentId)
            return BadRequest("El ID del estudiante no coincide");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        return Ok(new { message = "Estudiante inscrito exitosamente" });
    }

    /// <summary>
    /// Obtiene el historial académico de un estudiante
    /// </summary>
    [HttpGet("{id:guid}/academic-record")]
    [ProducesResponseType(typeof(AcademicRecordDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AcademicRecordDto>> GetAcademicRecord(Guid id)
    {
        var query = new GetStudentAcademicRecordQuery(id);
        var record = await _mediator.Send(query);
        
        if (record == null)
            return NotFound($"Historial académico no encontrado para estudiante {id}");
            
        return Ok(record);
    }
}