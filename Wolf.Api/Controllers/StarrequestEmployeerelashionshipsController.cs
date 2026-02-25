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
public class StarrequestEmployeerelashionshipsController : ControllerBase
{
    private readonly IRepository<StarrequestEmployeerelashionship> _repository;
    private readonly INotificationService _notify;

    public StarrequestEmployeerelashionshipsController(
        IRepository<StarrequestEmployeerelashionship> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StarrequestEmployeerelashionshipDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new StarrequestEmployeerelashionshipDto
        {
            Requestid = e.Requestid,
            Employeeid = e.Employeeid,
            Starcolor = e.Starcolor
        });
        return Ok(dtos);
    }

    [HttpGet("{requestId}/{employeeId}")]
    public async Task<ActionResult<StarrequestEmployeerelashionshipDto>> GetById(int requestId, int employeeId)
    {
        var entity = await _repository.GetByIdAsync(requestId, employeeId);
        if (entity is null) return NotFound();

        return Ok(new StarrequestEmployeerelashionshipDto
        {
            Requestid = entity.Requestid,
            Employeeid = entity.Employeeid,
            Starcolor = entity.Starcolor
        });
    }

    [HttpPost]
    public async Task<ActionResult<StarrequestEmployeerelashionshipDto>> Create(
        CreateStarrequestEmployeerelashionshipDto dto)
    {
        var entity = new StarrequestEmployeerelashionship
        {
            Requestid = dto.Requestid,
            Employeeid = dto.Employeeid,
            Starcolor = dto.Starcolor
        };

        await _repository.AddAsync(entity);

        var result = new StarrequestEmployeerelashionshipDto
        {
            Requestid = entity.Requestid,
            Employeeid = entity.Employeeid,
            Starcolor = entity.Starcolor
        };
        await _notify.NotifyAsync("StarRequestEmployeeRel", "Created", result);
        return CreatedAtAction(nameof(GetById),
            new { requestId = entity.Requestid, employeeId = entity.Employeeid },
            result);
    }

    [HttpPut("{requestId}/{employeeId}")]
    public async Task<IActionResult> Update(int requestId, int employeeId,
        StarrequestEmployeerelashionshipDto dto)
    {
        var entity = await _repository.GetByIdAsync(requestId, employeeId);
        if (entity is null) return NotFound();

        entity.Starcolor = dto.Starcolor;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("StarRequestEmployeeRel", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{requestId}/{employeeId}")]
    public async Task<IActionResult> Delete(int requestId, int employeeId)
    {
        var entity = await _repository.GetByIdAsync(requestId, employeeId);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("StarRequestEmployeeRel", "Deleted", new { Requestid = requestId, Employeeid = employeeId });
        return NoContent();
    }
}
