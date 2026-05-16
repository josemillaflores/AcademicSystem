using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Application.Commands;
using PaymentService.Application.Queries;
using PaymentService.Application.DTOs;

namespace PaymentService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    /// Obtiene todos los pagos
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll()
    {
        var query = new GetAllPaymentsQuery();
        var payments = await _mediator.Send(query);
        return Ok(payments);
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
            return NotFound($"Pago con ID {id} no encontrado");
            
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
            return NotFound($"Pago con número {paymentNumber} no encontrado");
            
        return Ok(payment);
    }

    /// <summary>
    /// Crea un nuevo pago
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreatePaymentCommand command)
    {
        try
        {
            var paymentId = await _mediator.Send(command);
            _logger.LogInformation("Pago creado con ID: {PaymentId}", paymentId);
            return CreatedAtAction(nameof(GetById), new { id = paymentId }, paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear pago");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Procesa un pago
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
            return BadRequest(result.Error);
            
        _logger.LogInformation("Pago procesado: {PaymentId}", id);
        return Ok(new { message = "Pago procesado exitosamente" });
    }

    /// <summary>
    /// Completa un pago
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompletePaymentCommand command)
    {
        if (id != command.PaymentId)
            return BadRequest("El ID del pago no coincide");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        _logger.LogInformation("Pago completado: {PaymentId}", id);
        return Ok(new { message = "Pago completado exitosamente", transactionId = command.TransactionId });
    }

    /// <summary>
    /// Rechaza un pago
    /// </summary>
    [HttpPost("{id:guid}/fail")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Fail(Guid id, [FromBody] FailPaymentCommand command)
    {
        if (id != command.PaymentId)
            return BadRequest("El ID del pago no coincide");

        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(result.Error);
            
        _logger.LogInformation("Pago fallido: {PaymentId}", id);
        return Ok(new { message = "Pago marcado como fallido", reason = command.Reason });
    }