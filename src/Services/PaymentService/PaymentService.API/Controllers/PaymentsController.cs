using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands;
using PaymentService.Application.DTOs;
using PaymentService.Application.Queries;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Default")]
[Produces("application/json")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IMediator mediator, ILogger<PaymentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los pagos con paginación
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<PaymentDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? status = null,
        [FromQuery] Guid? studentId = null)
    {
        var query = new GetAllPaymentsQuery(page, pageSize, status, studentId);
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
    /// Obtiene un pago por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetById(Guid id)
    {
        var query = new GetPaymentQuery(id);
        var payment = await _mediator.Send(query);
        
        if (payment == null)
            return NotFound(new { error = $"Payment with ID {id} not found" });
            
        return Ok(payment);
    }

    /// <summary>
    /// Obtiene un pago por número de pago
    /// </summary>
    [HttpGet("by-number/{paymentNumber}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetByNumber(string paymentNumber)
    {
        var query = new GetPaymentByNumberQuery(paymentNumber);
        var payment = await _mediator.Send(query);
        
        if (payment == null)
            return NotFound(new { error = $"Payment with number {paymentNumber} not found" });
            
        return Ok(payment);
    }

    /// <summary>
    /// Obtiene pagos por estudiante
    /// </summary>
    [HttpGet("student/{studentId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetByStudent(Guid studentId)
    {
        var query = new GetPaymentsByStudentQuery(studentId);
        var payments = await _mediator.Send(query);
        return Ok(payments);
    }

    /// <summary>
    /// Crea un nuevo pago
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        _logger.LogInformation("Payment created with ID: {PaymentId}, Number: {PaymentNumber}", 
            result.Data, result.PaymentNumber);
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, 
            new { id = result.Data, paymentNumber = result.PaymentNumber });
    }

    /// <summary>
    /// Procesa un pago (inicia el proceso de pago)
    /// </summary>
    [HttpPost("{id:guid}/process")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Process(Guid id)
    {
        var command = new ProcessPaymentCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
        {
            if (result.Error.Contains("not found"))
                return NotFound(new { error = result.Error });
            return BadRequest(new { error = result.Error });
        }
            
        _logger.LogInformation("Payment processed: {PaymentId}", id);
        return Ok(new { message = "Payment processed successfully" });
    }

    /// <summary>
    /// Completa un pago (marca como pagado)
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompletePaymentCommand command)
    {
        if (id != command.PaymentId)
            return BadRequest(new { error = "Route ID does not match payment ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        _logger.LogInformation("Payment completed: {PaymentId}, TransactionId: {TransactionId}", 
            id, command.TransactionId);
            
        return Ok(new { message = "Payment completed successfully", transactionId = command.TransactionId });
    }

    /// <summary>
    /// Marca un pago como fallido
    /// </summary>
    [HttpPost("{id:guid}/fail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Fail(Guid id, [FromBody] FailPaymentCommand command)
    {
        if (id != command.PaymentId)
            return BadRequest(new { error = "Route ID does not match payment ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        _logger.LogInformation("Payment failed: {PaymentId}, Reason: {Reason}", id, command.Reason);
        return Ok(new { message = "Payment marked as failed", reason = command.Reason });
    }

    /// <summary>
    /// Reembolsa un pago
    /// </summary>
    [HttpPost("{id:guid}/refund")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refund(Guid id, [FromBody] RefundPaymentCommand command)
    {
        if (id != command.PaymentId)
            return BadRequest(new { error = "Route ID does not match payment ID" });

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        _logger.LogInformation("Payment refunded: {PaymentId}, Amount: {Amount}", id, command.Amount);
        return Ok(new { message = "Payment refunded successfully", refundId = result.Data });
    }

    /// <summary>
    /// Obtiene el historial de transacciones de un pago
    /// </summary>
    [HttpGet("{id:guid}/transactions")]
    [ProducesResponseType(typeof(IEnumerable<TransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactions(Guid id)
    {
        var query = new GetPaymentTransactionsQuery(id);
        var transactions = await _mediator.Send(query);
        return Ok(transactions);
    }

    /// <summary>
    /// Obtiene el balance de un estudiante
    /// </summary>
    [HttpGet("student/{studentId:guid}/balance")]
    [ProducesResponseType(typeof(StudentBalanceDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<StudentBalanceDto>> GetStudentBalance(Guid studentId)
    {
        var query = new GetStudentBalanceQuery(studentId);
        var balance = await _mediator.Send(query);
        return Ok(balance);
    }

    /// <summary>
    /// Obtiene estadísticas de pagos
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(PaymentStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentStatisticsDto>> GetStatistics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var query = new GetPaymentStatisticsQuery(startDate, endDate);
        var statistics = await _mediator.Send(query);
        return Ok(statistics);
    }

    /// <summary>
    /// Obtiene el resumen de pagos por método
    /// </summary>
    [HttpGet("summary/by-method")]
    [ProducesResponseType(typeof(IEnumerable<PaymentMethodSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentMethodSummaryDto>>> GetSummaryByMethod()
    {
        var query = new GetPaymentMethodSummaryQuery();
        var summary = await _mediator.Send(query);
        return Ok(summary);
    }

    /// <summary>
    /// Exporta pagos a Excel
    /// </summary>
    [HttpGet("export/excel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportToExcel([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var query = new ExportPaymentsQuery(startDate, endDate);
        var excelData = await _mediator.Send(query);
        
        return File(excelData, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
            $"payments_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}