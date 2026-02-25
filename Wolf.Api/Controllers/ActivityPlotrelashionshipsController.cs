using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Wolf.Api.Repositories;
using Wolf.Api.Services;
using Wolf.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Wolf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ActivityPlotrelashionshipsController : ControllerBase
{
    private readonly IRepository<ActivityPlotrelashionship> _repository;
    private readonly INotificationService _notify;

    public ActivityPlotrelashionshipsController(IRepository<ActivityPlotrelashionship> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityPlotrelashionshipDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new ActivityPlotrelashionshipDto
        {
            Activityid = e.Activityid,
            Plotid = e.Plotid
        });
        return Ok(dtos);
    }

    [HttpGet("{activityId}/{plotId}")]
    public async Task<ActionResult<ActivityPlotrelashionshipDto>> GetById(int activityId, int plotId)
    {
        var entity = await _repository.GetByIdAsync(activityId, plotId);
        if (entity is null) return NotFound();

        return Ok(new ActivityPlotrelashionshipDto
        {
            Activityid = entity.Activityid,
            Plotid = entity.Plotid
        });
    }

    [HttpPost]
    public async Task<ActionResult<ActivityPlotrelashionshipDto>> Create(CreateActivityPlotrelashionshipDto dto)
    {
        var entity = new ActivityPlotrelashionship
        {
            Activityid = dto.Activityid,
            Plotid = dto.Plotid
        };

        await _repository.AddAsync(entity);

        var result = new ActivityPlotrelashionshipDto
        {
            Activityid = entity.Activityid,
            Plotid = entity.Plotid
        };
        await _notify.NotifyAsync("ActivityPlotRel", "Created", result);
        return CreatedAtAction(nameof(GetById),
            new { activityId = entity.Activityid, plotId = entity.Plotid },
            result);
    }

    [HttpDelete("{activityId}/{plotId}")]
    public async Task<IActionResult> Delete(int activityId, int plotId)
    {
        var entity = await _repository.GetByIdAsync(activityId, plotId);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("ActivityPlotRel", "Deleted", new { Activityid = activityId, Plotid = plotId });
        return NoContent();
    }
}
