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
public class OwnersController : ControllerBase
{
    private readonly IRepository<Owner> _repository;
    private readonly INotificationService _notify;

    public OwnersController(IRepository<Owner> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OwnerDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new OwnerDto
        {
            Ownerid = e.Ownerid,
            Egn = e.Egn,
            Address = e.Address,
            Fullname = e.Fullname
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OwnerDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new OwnerDto
        {
            Ownerid = entity.Ownerid,
            Egn = entity.Egn,
            Address = entity.Address,
            Fullname = entity.Fullname
        });
    }

    [HttpPost]
    public async Task<ActionResult<OwnerDto>> Create(CreateOwnerDto dto)
    {
        var entity = new Owner
        {
            Egn = dto.Egn,
            Address = dto.Address,
            Fullname = dto.Fullname
        };

        await _repository.AddAsync(entity);

        var result = new OwnerDto
        {
            Ownerid = entity.Ownerid,
            Egn = entity.Egn,
            Address = entity.Address,
            Fullname = entity.Fullname
        };
        await _notify.NotifyAsync("Owner", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Ownerid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, OwnerDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Egn = dto.Egn;
        entity.Address = dto.Address;
        entity.Fullname = dto.Fullname;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("Owner", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("Owner", "Deleted", new { Id = id });
        return NoContent();
    }
}
