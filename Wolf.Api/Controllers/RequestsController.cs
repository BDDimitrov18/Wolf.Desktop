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
public class RequestsController : ControllerBase
{
    private readonly IRepository<Request> _repository;
    private readonly INotificationService _notify;
    private readonly WolfDbContext _context;

    public RequestsController(IRepository<Request> repository, INotificationService notify,
        WolfDbContext context)
    {
        _repository = repository;
        _notify = notify;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RequestDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new RequestDto
        {
            Requestid = e.Requestid,
            Requestname = e.Requestname,
            Price = e.Price,
            Paymentstatus = e.Paymentstatus,
            Advance = e.Advance,
            Comments = e.Comments,
            Path = e.Path,
            Requestcreatorid = e.Requestcreatorid,
            Status = e.Status
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<RequestDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new RequestDto
        {
            Requestid = entity.Requestid,
            Requestname = entity.Requestname,
            Price = entity.Price,
            Paymentstatus = entity.Paymentstatus,
            Advance = entity.Advance,
            Comments = entity.Comments,
            Path = entity.Path,
            Requestcreatorid = entity.Requestcreatorid,
            Status = entity.Status
        });
    }

    [HttpPost]
    public async Task<ActionResult<RequestDto>> Create(CreateRequestDto dto)
    {
        var entity = new Request
        {
            Requestname = dto.Requestname,
            Price = dto.Price,
            Paymentstatus = dto.Paymentstatus,
            Advance = dto.Advance,
            Comments = dto.Comments,
            Path = dto.Path,
            Requestcreatorid = dto.Requestcreatorid,
            Status = dto.Status
        };

        await _repository.AddAsync(entity);

        var result = new RequestDto
        {
            Requestid = entity.Requestid,
            Requestname = entity.Requestname,
            Price = entity.Price,
            Paymentstatus = entity.Paymentstatus,
            Advance = entity.Advance,
            Comments = entity.Comments,
            Path = entity.Path,
            Requestcreatorid = entity.Requestcreatorid,
            Status = entity.Status
        };
        await _notify.NotifyAsync("Request", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Requestid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, RequestDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Requestname = dto.Requestname;
        entity.Price = dto.Price;
        entity.Paymentstatus = dto.Paymentstatus;
        entity.Advance = dto.Advance;
        entity.Comments = dto.Comments;
        entity.Path = dto.Path;
        entity.Requestcreatorid = dto.Requestcreatorid;
        entity.Status = dto.Status;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("Request", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Requests.FindAsync(id);
        if (entity is null) return NotFound();

        // Gather all children
        var activities = await _context.Activities
            .Where(a => a.Requestid == id).ToListAsync();
        var activityIds = activities.Select(a => a.Activityid).ToList();

        var tasks = await _context.Tasks
            .Where(t => activityIds.Contains(t.Activityid)).ToListAsync();
        var actPlotRels = await _context.ActivityPlotrelashionships
            .Where(r => activityIds.Contains(r.Activityid)).ToListAsync();
        var clientReqRels = await _context.ClientRequestrelashionships
            .Where(r => r.Requestid == id).ToListAsync();
        var starReqRels = await _context.StarrequestEmployeerelashionships
            .Where(r => r.Requestid == id).ToListAsync();
        var invoices = await _context.Invoices
            .Where(i => i.Requestid == id).ToListAsync();

        // Remove in dependency order
        _context.Tasks.RemoveRange(tasks);
        _context.ActivityPlotrelashionships.RemoveRange(actPlotRels);
        foreach (var a in activities) a.Parentactivityid = null;
        _context.Activities.RemoveRange(activities);
        _context.ClientRequestrelashionships.RemoveRange(clientReqRels);
        _context.StarrequestEmployeerelashionships.RemoveRange(starReqRels);
        _context.Invoices.RemoveRange(invoices);
        _context.Requests.Remove(entity);
        await _context.SaveChangesAsync();

        // Notify all deletions
        foreach (var t in tasks)
            await _notify.NotifyAsync("Task", "Deleted", new { Id = t.Taskid });
        foreach (var r in actPlotRels)
            await _notify.NotifyAsync("ActivityPlotRel", "Deleted",
                new { r.Activityid, r.Plotid });
        foreach (var a in activities)
            await _notify.NotifyAsync("Activity", "Deleted", new { Id = a.Activityid });
        foreach (var r in clientReqRels)
            await _notify.NotifyAsync("ClientRequestRel", "Deleted",
                new { r.Requestid, r.Clientid });
        foreach (var i in invoices)
            await _notify.NotifyAsync("Invoice", "Deleted", new { Id = i.Invoiceid });
        await _notify.NotifyAsync("Request", "Deleted", new { Id = id });

        return NoContent();
    }
}
