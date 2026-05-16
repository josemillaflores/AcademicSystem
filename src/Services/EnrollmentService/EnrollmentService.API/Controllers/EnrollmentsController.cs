using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnrollmentService.Application.Commands;
using EnrollmentService.Application.DTOs;
using EnrollmentService.Application.Queries;
using EnrollmentService.Application.Services;

namespace EnrollmentService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Default")]
[Produces("application/json")]
public class EnrollmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEnrollmentCompositionService _compositionService;
    private readonly ILogger<EnrollmentsController> _logger;

    public EnrollmentsController(
        IMediator mediator,
        IEnrollmentCompositionService compositionService,
        ILogger<EnrollmentsController> logger)
    {
        _mediator = mediator;
        _compositionService = compositionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todas las matrículas con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<EnrollmentDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null)
    {
        var query = new GetAllEnrollmentsQuery(page, pageSize, status);
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
            return NotFound(new { error = $"Enrollment with ID {id} not found" });
            
        return Ok(enrollment);
    }

    /// <summary>
    /// Obtiene información completa de una matrícula (API Composition)
    /// </summary>
    [HttpGet("{id:guid}/complete")]
    [ProducesResponseType(typeof(CompleteEnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompleteEnrollmentDto>> GetCompleteEnrollmentInfo(Guid id)
    {
        var result = await _compositionService.GetCompleteEnrollmentInfoAsync(id);
        
        if (result.Enrollment == null)
            return NotFound(new { error = $"Enrollment with ID {id} not found" });
            
        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva solicitud de matrícula
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateEnrollmentCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        _logger.LogInformation("Enrollment request created with ID: {EnrollmentId}", result.Data);
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
    }

    /// <summary>
    /// Procesa una matrícula completa (orquestación de múltiples servicios)
    /// </summary>
    [HttpPost("process")]
    [ProducesResponseType(typeof(ProcessEnrollmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProcessEnrollmentResult>> ProcessEnrollment([FromBody] ProcessEnrollmentRequest request)
    {
        var result = await _compositionService.ProcessEnrollmentAsync(request);
        
        if (!result.Success)
            return BadRequest(new { errors = result.Errors });
            
        _logger.LogInformation("Enrollment processed successfully: {EnrollmentId}", result.EnrollmentId);
        return Ok(result);
    }

    /// <summary>
    /// Valida una matrícula antes de procesarla
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(EnrollmentValidationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnrollmentValidationResult>> ValidateEnrollment(
        [FromQuery] Guid studentId, 
        [FromQuery] Guid courseId)
    {
        var result = await _compositionService.ValidateEnrollmentAsync(studentId, courseId);
        return Ok(result);
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
        {
            if (result.Error.Contains("not found"))
                return NotFound(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }
            
        _logger.LogInformation("Enrollment approved: {EnrollmentId}", id);
        return Ok(new { message = "Enrollment approved successfully" });
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
            return BadRequest(new { error = "Route ID does not match enrollment ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        _logger.LogInformation("Enrollment rejected: {EnrollmentId}, Reason: {Reason}", id, command.Reason);
        return Ok(new { message = "Enrollment rejected", reason = command.Reason });
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
            return BadRequest(new { error = result.Error });
            
        _logger.LogInformation("Enrollment cancelled: {EnrollmentId}", id);
        return Ok(new { message = "Enrollment cancelled successfully" });
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
    /// Obtiene matrículas por período
    /// </summary>
    [HttpGet("by-period")]
    [ProducesResponseType(typeof(IEnumerable<EnrollmentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<EnrollmentDto>>> GetByPeriod(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        var query = new GetEnrollmentsByPeriodQuery(startDate, endDate);
        var enrollments = await _mediator.Send(query);
        return Ok(enrollments);
    }

    /// <summary>
    /// Obtiene estadísticas de matrículas
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(EnrollmentStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EnrollmentStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? startDate = null, 
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetEnrollmentStatisticsQuery(startDate, endDate);
        var statistics = await _mediator.Send(query);
        return Ok(statistics);
    }

    /// <summary>
    /// Obtiene el resumen de matrículas por curso
    /// </summary>
    [HttpGet("summary/by-course")]
    [ProducesResponseType(typeof(IEnumerable<CourseEnrollmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CourseEnrollmentSummaryDto>>> GetSummaryByCourse()
    {
        var query = new GetCourseEnrollmentSummaryQuery();
        var summary = await _mediator.Send(query);
        return Ok(summary);
    }
}