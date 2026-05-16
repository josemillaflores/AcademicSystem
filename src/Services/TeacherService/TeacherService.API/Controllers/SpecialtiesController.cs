using MediatR;
using Microsoft.AspNetCore.Mvc;
using TeacherService.Application.Commands;
using TeacherService.Application.Queries;
using TeacherService.Application.DTOs;

namespace TeacherService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SpecialtiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SpecialtiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SpecialtyDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<SpecialtyDto>>> GetAll()
    {
        var query = new GetAllSpecialtiesQuery();
        var specialties = await _mediator.Send(query);
        return Ok(specialties);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SpecialtyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SpecialtyDto>> GetById(Guid id)
    {
        var query = new GetSpecialtyQuery(id);
        var specialty = await _mediator.Send(query);
        
        if (specialty == null)
            return NotFound();
            
        return Ok(specialty);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateSpecialtyCommand command)
    {
        var specialtyId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = specialtyId }, specialtyId);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSpecialtyCommand command)
    {
        if (id != command.Id)
            return BadRequest();

        await _mediator.Send(command);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteSpecialtyCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}