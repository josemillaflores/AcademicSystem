using MediatR;
using Microsoft.AspNetCore.Mvc;
using EnrollmentService.Application.Commands;
using EnrollmentService.Application.Queries;
using EnrollmentService.Application.DTOs;

namespace EnrollmentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EnrollmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EnrollmentsController> _logger;

    public EnrollmentsController(IMediator mediator, ILogger<EnrollmentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las matrículas
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetAll()
    {
        var query = new GetAllEnrollmentsQuery();
        var enrollments = await _mediator.Send(query);
        return Ok(enrollments);
    }

    /// <summary>
    /// Obtiene una matrícula por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EnrollmentDto>> GetById(Guid id)
    {
        var query = new GetEnrollmentQuery(id);
        var enrollment = await _mediator.Send(query);
        
        if (enrollment == null)
            return NotFound($"Matrícula con ID {id} no encontrada");
            
        return Ok(enrollment);
    }

    /// <summary>
    /// Crea una nueva solicitud de matrícula
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateEnrollmentCommand command)
    {
        try
        {
            var enrollmentId = await _mediator.Send(command);
            _logger.LogInformation("Solicitud de matrícula creada con ID: {EnrollmentId}", enrollmentId);
            return CreatedAtAction(nameof(GetById), new { id = enrollmentId }, enrollmentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear solicitud de matrícula");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Aprueba una matrícula
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Approve(Guid id)
    {
        var command = new ApproveEnrollmentCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        _logger.LogInformation("Matrícula aprobada: {EnrollmentId}", id);
        return Ok(new { message = "Matrícula aprobada exitosamente" });
    }

    /// <summary>
    /// Rechaza una matrícula
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectEnrollmentCommand command)
    {
        if (id != command.EnrollmentId)
            return BadRequest("El ID de la matrícula no coincide");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        _logger.LogInformation("Matrícula rechazada: {EnrollmentId}", id);
        return Ok(new { message = "Matrícula rechazada", reason = command.Reason });
    }

    /// <summary>
    /// Cancela una matrícula
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var command = new CancelEnrollmentCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        _logger.LogInformation("Matrícula cancelada: {EnrollmentId}", id);
        return Ok(new { message = "Matrícula cancelada exitosamente" });
    }

    /// <summary>
    /// Obtiene matrículas por estudiante
    /// </summary>
    [HttpGet("student/{studentId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetByStudent(Guid studentId)
    {
        var query = new GetEnrollmentsByStudentQuery(studentId);
        var enrollments = await _mediator.Send(query);
        return Ok(enrollments);
    }

    /// <summary>
    /// Obtiene matrículas por curso
    /// </summary>
    [HttpGet("course/{courseId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetByCourse(Guid courseId)
    {
        var query = new GetEnrollmentsByCourseQuery(courseId);
        var enrollments = await _mediator.Send(query);
        return Ok(enrollments);
    }

    /// <summary>
    /// Obtiene matrículas por estado
    /// </summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetByStatus(string status)
    {
        var query = new GetEnrollmentsByStatusQuery(status);
        var enrollments = await _mediator.Send(query);
        return Ok(enrollments);
    }

    /// <summary>
    /// Valida los prerrequisitos para una matrícula
    /// </summary>
    [HttpGet("validate-prerequisites")]
    [ProducesResponseType(typeof(ValidationResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ValidationResultDto>> ValidatePrerequisites([FromQuery] Guid studentId, [FromQuery] Guid courseId)
    {
        var query = new ValidatePrerequisitesQuery(studentId, courseId);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Obtiene estadísticas de matrículas
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(EnrollmentStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnrollmentStatisticsDto>> GetStatistics([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = new GetEnrollmentStatisticsQuery(startDate, endDate);
        var statistics = await _mediator.Send(query);
        return Ok(statistics);
    }
}