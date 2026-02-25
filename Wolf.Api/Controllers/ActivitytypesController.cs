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
public class ActivitytypesController : ControllerBase
{
    private readonly IRepository<Activitytype> _repository;
    private readonly INotificationService _notify;

    public ActivitytypesController(IRepository<Activitytype> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivitytypeDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new ActivitytypeDto
        {
            Activitytypeid = e.Activitytypeid,
            Activitytypename = e.Activitytypename
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActivitytypeDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new ActivitytypeDto
        {
            Activitytypeid = entity.Activitytypeid,
            Activitytypename = entity.Activitytypename
        });
    }

    [HttpPost]
    public async Task<ActionResult<ActivitytypeDto>> Create(CreateActivitytypeDto dto)
    {
        var entity = new Activitytype
        {
            Activitytypename = dto.Activitytypename
        };

        await _repository.AddAsync(entity);

        var result = new ActivitytypeDto
        {
            Activitytypeid = entity.Activitytypeid,
            Activitytypename = entity.Activitytypename
        };
        await _notify.NotifyAsync("ActivityType", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Activitytypeid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ActivitytypeDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Activitytypename = dto.Activitytypename;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("ActivityType", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("ActivityType", "Deleted", new { Id = id });
        return NoContent();
    }
}
