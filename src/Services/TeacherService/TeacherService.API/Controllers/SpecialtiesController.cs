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
public class SpecialtiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpecialtiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Obtiene todas las especialidades
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpecialtyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SpecialtyDto>>> GetAll()
    {
        var query = new GetAllSpecialtiesQuery();
        var specialties = await _mediator.Send(query);
        return Ok(specialties);
    }

    /// <summary>
    /// Obtiene una especialidad por ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpecialtyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpecialtyDto>> GetById(Guid id)
    {
        var query = new GetSpecialtyQuery(id);
        var specialty = await _mediator.Send(query);
        
        if (specialty == null)
            return NotFound(new { error = $"Specialty with ID {id} not found" });
            
        return Ok(specialty);
    }

    /// <summary>
    /// Crea una nueva especialidad
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateSpecialtyCommand command)
    {
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });
            
        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data);
    }

    /// <summary>
    /// Actualiza una especialidad
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpecialtyCommand command)
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
            
        return NoContent();
    }

    /// <summary>
    /// Elimina una especialidad
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteSpecialtyCommand(id);
        var result = await _mediator.Send(command);
        
        if (!result.IsSuccess)
            return NotFound(new { error = result.Error });
            
        return NoContent();
    }
}