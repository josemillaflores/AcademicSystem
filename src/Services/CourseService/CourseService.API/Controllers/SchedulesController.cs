using MediatR;
using Microsoft.AspNetCore.Mvc;
using CourseService.Application.Commands;
using CourseService.Application.Queries;
using CourseService.Application.DTOs;

namespace CourseService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SchedulesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("course/{courseId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ScheduleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ScheduleDto>>> GetByCourse(Guid courseId)
    {
        var query = new GetCourseSchedulesQuery(courseId);
        var schedules = await _mediator.Send(query);
        return Ok(schedules);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateScheduleCommand command)
    {
        var scheduleId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = scheduleId }, scheduleId);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ScheduleDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ScheduleDto>> GetById(Guid id)
    {
        var query = new GetScheduleQuery(id);
        var schedule = await _mediator.Send(query);
        return Ok(schedule);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateScheduleCommand command)
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
        var command = new DeleteScheduleCommand(id);
        await _mediator.Send(command);
        return NoContent();
    }
}