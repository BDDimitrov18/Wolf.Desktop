using DataLayer.Context;
using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wolf.Api.Repositories;
using Wolf.Api.Services;
using Wolf.Dtos;
using Microsoft.AspNetCore.Authorization;

namespace Wolf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ActivitiesController : ControllerBase
{
    private readonly IRepository<Activity> _repository;
    private readonly INotificationService _notify;
    private readonly WolfDbContext _context;

    public ActivitiesController(IRepository<Activity> repository, INotificationService notify,
        WolfDbContext context)
    {
        _repository = repository;
        _notify = notify;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new ActivityDto
        {
            Activityid = e.Activityid,
            Requestid = e.Requestid,
            Activitytypeid = e.Activitytypeid,
            Expectedduration = e.Expectedduration,
            Parentactivityid = e.Parentactivityid,
            Executantid = e.Executantid,
            Employeepayment = e.Employeepayment,
            Startdate = e.Startdate
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ActivityDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new ActivityDto
        {
            Activityid = entity.Activityid,
            Requestid = entity.Requestid,
            Activitytypeid = entity.Activitytypeid,
            Expectedduration = entity.Expectedduration,
            Parentactivityid = entity.Parentactivityid,
            Executantid = entity.Executantid,
            Employeepayment = entity.Employeepayment,
            Startdate = entity.Startdate
        });
    }

    [HttpPost]
    public async Task<ActionResult<ActivityDto>> Create(CreateActivityDto dto)
    {
        var entity = new Activity
        {
            Requestid = dto.Requestid,
            Activitytypeid = dto.Activitytypeid,
            Expectedduration = dto.Expectedduration,
            Parentactivityid = dto.Parentactivityid,
            Executantid = dto.Executantid,
            Employeepayment = dto.Employeepayment,
            Startdate = dto.Startdate
        };

        await _repository.AddAsync(entity);

        var result = new ActivityDto
        {
            Activityid = entity.Activityid,
            Requestid = entity.Requestid,
            Activitytypeid = entity.Activitytypeid,
            Expectedduration = entity.Expectedduration,
            Parentactivityid = entity.Parentactivityid,
            Executantid = entity.Executantid,
            Employeepayment = entity.Employeepayment,
            Startdate = entity.Startdate
        };
        await _notify.NotifyAsync("Activity", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Activityid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ActivityDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Requestid = dto.Requestid;
        entity.Activitytypeid = dto.Activitytypeid;
        entity.Expectedduration = dto.Expectedduration;
        entity.Parentactivityid = dto.Parentactivityid;
        entity.Executantid = dto.Executantid;
        entity.Employeepayment = dto.Employeepayment;
        entity.Startdate = dto.Startdate;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("Activity", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Activities.FindAsync(id);
        if (entity is null) return NotFound();

        // Cascade: delete all tasks for this activity
        var tasks = await _context.Tasks
            .Where(t => t.Activityid == id).ToListAsync();
        var actPlotRels = await _context.ActivityPlotrelashionships
            .Where(r => r.Activityid == id).ToListAsync();

        // Set child activities' parent to null
        var childActivities = await _context.Activities
            .Where(a => a.Parentactivityid == id).ToListAsync();
        foreach (var child in childActivities)
            child.Parentactivityid = null;

        _context.Tasks.RemoveRange(tasks);
        _context.ActivityPlotrelashionships.RemoveRange(actPlotRels);
        _context.Activities.Remove(entity);
        await _context.SaveChangesAsync();

        // Notify all deletions
        foreach (var t in tasks)
            await _notify.NotifyAsync("Task", "Deleted", new { Id = t.Taskid });
        foreach (var r in actPlotRels)
            await _notify.NotifyAsync("ActivityPlotRel", "Deleted",
                new { r.Activityid, r.Plotid });
        await _notify.NotifyAsync("Activity", "Deleted", new { Id = id });

        return NoContent();
    }
}
