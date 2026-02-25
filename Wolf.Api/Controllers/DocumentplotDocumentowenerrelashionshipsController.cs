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
public class DocumentplotDocumentowenerrelashionshipsController : ControllerBase
{
    private readonly IRepository<DocumentplotDocumentowenerrelashionship> _repository;
    private readonly INotificationService _notify;

    public DocumentplotDocumentowenerrelashionshipsController(
        IRepository<DocumentplotDocumentowenerrelashionship> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentplotDocumentowenerrelashionshipDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new DocumentplotDocumentowenerrelashionshipDto
        {
            Id = e.Id,
            Documentplotid = e.Documentplotid,
            Documentownerid = e.Documentownerid,
            Idealparts = e.Idealparts,
            Wayofacquiring = e.Wayofacquiring,
            Isdrob = e.Isdrob,
            Powerofattorneyid = e.Powerofattorneyid
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentplotDocumentowenerrelashionshipDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new DocumentplotDocumentowenerrelashionshipDto
        {
            Id = entity.Id,
            Documentplotid = entity.Documentplotid,
            Documentownerid = entity.Documentownerid,
            Idealparts = entity.Idealparts,
            Wayofacquiring = entity.Wayofacquiring,
            Isdrob = entity.Isdrob,
            Powerofattorneyid = entity.Powerofattorneyid
        });
    }

    [HttpPost]
    public async Task<ActionResult<DocumentplotDocumentowenerrelashionshipDto>> Create(
        CreateDocumentplotDocumentowenerrelashionshipDto dto)
    {
        var entity = new DocumentplotDocumentowenerrelashionship
        {
            Documentplotid = dto.Documentplotid,
            Documentownerid = dto.Documentownerid,
            Idealparts = dto.Idealparts,
            Wayofacquiring = dto.Wayofacquiring,
            Isdrob = dto.Isdrob,
            Powerofattorneyid = dto.Powerofattorneyid
        };

        await _repository.AddAsync(entity);

        var result = new DocumentplotDocumentowenerrelashionshipDto
        {
            Id = entity.Id,
            Documentplotid = entity.Documentplotid,
            Documentownerid = entity.Documentownerid,
            Idealparts = entity.Idealparts,
            Wayofacquiring = entity.Wayofacquiring,
            Isdrob = entity.Isdrob,
            Powerofattorneyid = entity.Powerofattorneyid
        };
        await _notify.NotifyAsync("DocPlotDocOwnerRel", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, DocumentplotDocumentowenerrelashionshipDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Documentplotid = dto.Documentplotid;
        entity.Documentownerid = dto.Documentownerid;
        entity.Idealparts = dto.Idealparts;
        entity.Wayofacquiring = dto.Wayofacquiring;
        entity.Isdrob = dto.Isdrob;
        entity.Powerofattorneyid = dto.Powerofattorneyid;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("DocPlotDocOwnerRel", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("DocPlotDocOwnerRel", "Deleted", new { Id = id });
        return NoContent();
    }
}
