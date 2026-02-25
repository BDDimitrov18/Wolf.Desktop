using Microsoft.AspNetCore.Mvc;
using Wolf.Api.Repositories;
using Wolf.Api.Services;
using Wolf.Dtos;
using TaskEntity = DataLayer.Entities.Task;
using Microsoft.AspNetCore.Authorization;

namespace Wolf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IRepository<TaskEntity> _repository;
    private readonly INotificationService _notify;

    public TasksController(IRepository<TaskEntity> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new TaskDto
        {
            Taskid = e.Taskid,
            Activityid = e.Activityid,
            Duration = e.Duration,
            Startdate = e.Startdate,
            Executantid = e.Executantid,
            Controlid = e.Controlid,
            Comments = e.Comments,
            Tasktypeid = e.Tasktypeid,
            Commenttax = e.Commenttax,
            Executantpayment = e.Executantpayment,
            Tax = e.Tax,
            Finishdate = e.Finishdate,
            Status = e.Status
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TaskDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new TaskDto
        {
            Taskid = entity.Taskid,
            Activityid = entity.Activityid,
            Duration = entity.Duration,
            Startdate = entity.Startdate,
            Executantid = entity.Executantid,
            Controlid = entity.Controlid,
            Comments = entity.Comments,
            Tasktypeid = entity.Tasktypeid,
            Commenttax = entity.Commenttax,
            Executantpayment = entity.Executantpayment,
            Tax = entity.Tax,
            Finishdate = entity.Finishdate,
            Status = entity.Status
        });
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(CreateTaskDto dto)
    {
        var entity = new TaskEntity
        {
            Activityid = dto.Activityid,
            Duration = dto.Duration,
            Startdate = dto.Startdate,
            Executantid = dto.Executantid,
            Controlid = dto.Controlid,
            Comments = dto.Comments,
            Tasktypeid = dto.Tasktypeid,
            Commenttax = dto.Commenttax,
            Executantpayment = dto.Executantpayment,
            Tax = dto.Tax,
            Finishdate = dto.Finishdate,
            Status = dto.Status
        };

        await _repository.AddAsync(entity);

        var result = new TaskDto
        {
            Taskid = entity.Taskid,
            Activityid = entity.Activityid,
            Duration = entity.Duration,
            Startdate = entity.Startdate,
            Executantid = entity.Executantid,
            Controlid = entity.Controlid,
            Comments = entity.Comments,
            Tasktypeid = entity.Tasktypeid,
            Commenttax = entity.Commenttax,
            Executantpayment = entity.Executantpayment,
            Tax = entity.Tax,
            Finishdate = entity.Finishdate,
            Status = entity.Status
        };
        await _notify.NotifyAsync("Task", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Taskid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TaskDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Activityid = dto.Activityid;
        entity.Duration = dto.Duration;
        entity.Startdate = dto.Startdate;
        entity.Executantid = dto.Executantid;
        entity.Controlid = dto.Controlid;
        entity.Comments = dto.Comments;
        entity.Tasktypeid = dto.Tasktypeid;
        entity.Commenttax = dto.Commenttax;
        entity.Executantpayment = dto.Executantpayment;
        entity.Tax = dto.Tax;
        entity.Finishdate = dto.Finishdate;
        entity.Status = dto.Status;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("Task", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("Task", "Deleted", new { Id = id });
        return NoContent();
    }
}
