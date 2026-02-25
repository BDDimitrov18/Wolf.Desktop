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
public class PowerofattorneydocumentsController : ControllerBase
{
    private readonly IRepository<Powerofattorneydocument> _repository;
    private readonly INotificationService _notify;

    public PowerofattorneydocumentsController(IRepository<Powerofattorneydocument> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PowerofattorneydocumentDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new PowerofattorneydocumentDto
        {
            Powerofattorneyid = e.Powerofattorneyid,
            Number = e.Number,
            Dateofissuing = e.Dateofissuing,
            Issuer = e.Issuer
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PowerofattorneydocumentDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new PowerofattorneydocumentDto
        {
            Powerofattorneyid = entity.Powerofattorneyid,
            Number = entity.Number,
            Dateofissuing = entity.Dateofissuing,
            Issuer = entity.Issuer
        });
    }

    [HttpPost]
    public async Task<ActionResult<PowerofattorneydocumentDto>> Create(CreatePowerofattorneydocumentDto dto)
    {
        var entity = new Powerofattorneydocument
        {
            Number = dto.Number,
            Dateofissuing = dto.Dateofissuing,
            Issuer = dto.Issuer
        };

        await _repository.AddAsync(entity);

        var result = new PowerofattorneydocumentDto
        {
            Powerofattorneyid = entity.Powerofattorneyid,
            Number = entity.Number,
            Dateofissuing = entity.Dateofissuing,
            Issuer = entity.Issuer
        };
        await _notify.NotifyAsync("PowerofattorneyDoc", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Powerofattorneyid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PowerofattorneydocumentDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Number = dto.Number;
        entity.Dateofissuing = dto.Dateofissuing;
        entity.Issuer = dto.Issuer;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("PowerofattorneyDoc", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("PowerofattorneyDoc", "Deleted", new { Id = id });
        return NoContent();
    }
}
