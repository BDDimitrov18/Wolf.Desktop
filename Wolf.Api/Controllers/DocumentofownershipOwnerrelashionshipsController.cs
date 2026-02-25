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
public class DocumentofownershipOwnerrelashionshipsController : ControllerBase
{
    private readonly IRepository<DocumentofownershipOwnerrelashionship> _repository;
    private readonly INotificationService _notify;
    private readonly WolfDbContext _context;

    public DocumentofownershipOwnerrelashionshipsController(
        IRepository<DocumentofownershipOwnerrelashionship> repository,
        INotificationService notify, WolfDbContext context)
    {
        _repository = repository;
        _notify = notify;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentofownershipOwnerrelashionshipDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new DocumentofownershipOwnerrelashionshipDto
        {
            Documentownerid = e.Documentownerid,
            Documentid = e.Documentid,
            Ownerid = e.Ownerid
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentofownershipOwnerrelashionshipDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new DocumentofownershipOwnerrelashionshipDto
        {
            Documentownerid = entity.Documentownerid,
            Documentid = entity.Documentid,
            Ownerid = entity.Ownerid
        });
    }

    [HttpPost]
    public async Task<ActionResult<DocumentofownershipOwnerrelashionshipDto>> Create(
        CreateDocumentofownershipOwnerrelashionshipDto dto)
    {
        var entity = new DocumentofownershipOwnerrelashionship
        {
            Documentid = dto.Documentid,
            Ownerid = dto.Ownerid
        };

        await _repository.AddAsync(entity);

        var result = new DocumentofownershipOwnerrelashionshipDto
        {
            Documentownerid = entity.Documentownerid,
            Documentid = entity.Documentid,
            Ownerid = entity.Ownerid
        };
        await _notify.NotifyAsync("DocOwnerRel", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Documentownerid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, DocumentofownershipOwnerrelashionshipDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Documentid = dto.Documentid;
        entity.Ownerid = dto.Ownerid;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("DocOwnerRel", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.DocumentofownershipOwnerrelashionships.FindAsync(id);
        if (entity is null) return NotFound();

        // Cascade-delete three-way records referencing this doc-owner link
        var threeWayRels = await _context.DocumentplotDocumentowenerrelashionships
            .Where(r => r.Documentownerid == id).ToListAsync();
        _context.DocumentplotDocumentowenerrelashionships.RemoveRange(threeWayRels);
        _context.DocumentofownershipOwnerrelashionships.Remove(entity);
        await _context.SaveChangesAsync();

        foreach (var r in threeWayRels)
            await _notify.NotifyAsync("DocPlotDocOwnerRel", "Deleted", new { r.Id });
        await _notify.NotifyAsync("DocOwnerRel", "Deleted",
            new { Documentownerid = id });
        return NoContent();
    }
}
