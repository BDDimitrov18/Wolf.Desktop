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
public class TasktypesController : ControllerBase
{
    private readonly IRepository<Tasktype> _repository;
    private readonly INotificationService _notify;

    public TasktypesController(IRepository<Tasktype> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TasktypeDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new TasktypeDto
        {
            Tasktypeid = e.Tasktypeid,
            Tasktypename = e.Tasktypename,
            Activitytypeid = e.Activitytypeid
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TasktypeDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new TasktypeDto
        {
            Tasktypeid = entity.Tasktypeid,
            Tasktypename = entity.Tasktypename,
            Activitytypeid = entity.Activitytypeid
        });
    }

    [HttpPost]
    public async Task<ActionResult<TasktypeDto>> Create(CreateTasktypeDto dto)
    {
        var entity = new Tasktype
        {
            Tasktypename = dto.Tasktypename,
            Activitytypeid = dto.Activitytypeid
        };

        await _repository.AddAsync(entity);

        var result = new TasktypeDto
        {
            Tasktypeid = entity.Tasktypeid,
            Tasktypename = entity.Tasktypename,
            Activitytypeid = entity.Activitytypeid
        };
        await _notify.NotifyAsync("TaskType", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Tasktypeid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TasktypeDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Tasktypename = dto.Tasktypename;
        entity.Activitytypeid = dto.Activitytypeid;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("TaskType", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("TaskType", "Deleted", new { Id = id });
        return NoContent();
    }
}
