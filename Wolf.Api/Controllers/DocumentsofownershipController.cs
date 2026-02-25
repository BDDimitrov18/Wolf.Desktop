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
public class DocumentsofownershipController : ControllerBase
{
    private readonly IRepository<Documentsofownership> _repository;
    private readonly INotificationService _notify;
    private readonly WolfDbContext _context;

    public DocumentsofownershipController(IRepository<Documentsofownership> repository,
        INotificationService notify, WolfDbContext context)
    {
        _repository = repository;
        _notify = notify;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DocumentsofownershipDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new DocumentsofownershipDto
        {
            Documentid = e.Documentid,
            Typeofdocument = e.Typeofdocument,
            Numberofdocument = e.Numberofdocument,
            Issuer = e.Issuer,
            Tom = e.Tom,
            Register = e.Register,
            Doccase = e.Doccase,
            Dateofissuing = e.Dateofissuing,
            Dateofregistering = e.Dateofregistering,
            Typeofownership = e.Typeofownership
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DocumentsofownershipDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new DocumentsofownershipDto
        {
            Documentid = entity.Documentid,
            Typeofdocument = entity.Typeofdocument,
            Numberofdocument = entity.Numberofdocument,
            Issuer = entity.Issuer,
            Tom = entity.Tom,
            Register = entity.Register,
            Doccase = entity.Doccase,
            Dateofissuing = entity.Dateofissuing,
            Dateofregistering = entity.Dateofregistering,
            Typeofownership = entity.Typeofownership
        });
    }

    [HttpPost]
    public async Task<ActionResult<DocumentsofownershipDto>> Create(CreateDocumentsofownershipDto dto)
    {
        var entity = new Documentsofownership
        {
            Typeofdocument = dto.Typeofdocument,
            Numberofdocument = dto.Numberofdocument,
            Issuer = dto.Issuer,
            Tom = dto.Tom,
            Register = dto.Register,
            Doccase = dto.Doccase,
            Dateofissuing = dto.Dateofissuing,
            Dateofregistering = dto.Dateofregistering,
            Typeofownership = dto.Typeofownership
        };

        await _repository.AddAsync(entity);

        var result = new DocumentsofownershipDto
        {
            Documentid = entity.Documentid,
            Typeofdocument = entity.Typeofdocument,
            Numberofdocument = entity.Numberofdocument,
            Issuer = entity.Issuer,
            Tom = entity.Tom,
            Register = entity.Register,
            Doccase = entity.Doccase,
            Dateofissuing = entity.Dateofissuing,
            Dateofregistering = entity.Dateofregistering,
            Typeofownership = entity.Typeofownership
        };
        await _notify.NotifyAsync("Document", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Documentid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, DocumentsofownershipDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Typeofdocument = dto.Typeofdocument;
        entity.Numberofdocument = dto.Numberofdocument;
        entity.Issuer = dto.Issuer;
        entity.Tom = dto.Tom;
        entity.Register = dto.Register;
        entity.Doccase = dto.Doccase;
        entity.Dateofissuing = dto.Dateofissuing;
        entity.Dateofregistering = dto.Dateofregistering;
        entity.Typeofownership = dto.Typeofownership;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("Document", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.Documentsofownerships.FindAsync(id);
        if (entity is null) return NotFound();

        // Cascade-delete relationships only (not owners or plots)
        var plotDocRels = await _context.PlotDocumentofownerships
            .Where(r => r.Documentofownershipid == id).ToListAsync();
        var plotDocRelIds = plotDocRels.Select(r => r.Documentplotid).ToList();

        var docOwnerRels = await _context.DocumentofownershipOwnerrelashionships
            .Where(r => r.Documentid == id).ToListAsync();
        var docOwnerRelIds = docOwnerRels.Select(r => r.Documentownerid).ToList();

        // Three-way records reference both PlotDocRels and DocOwnerRels — query by both FKs
        var docPlotDocOwnerRels = await _context.DocumentplotDocumentowenerrelashionships
            .Where(r => plotDocRelIds.Contains(r.Documentplotid) ||
                        docOwnerRelIds.Contains(r.Documentownerid))
            .ToListAsync();

        _context.DocumentplotDocumentowenerrelashionships.RemoveRange(docPlotDocOwnerRels);
        _context.DocumentofownershipOwnerrelashionships.RemoveRange(docOwnerRels);
        _context.PlotDocumentofownerships.RemoveRange(plotDocRels);
        _context.Documentsofownerships.Remove(entity);
        await _context.SaveChangesAsync();

        // Notify all deletions
        foreach (var r in docPlotDocOwnerRels)
            await _notify.NotifyAsync("DocPlotDocOwnerRel", "Deleted", new { r.Id });
        foreach (var r in docOwnerRels)
            await _notify.NotifyAsync("DocOwnerRel", "Deleted",
                new { r.Documentownerid });
        foreach (var r in plotDocRels)
            await _notify.NotifyAsync("PlotDocumentRel", "Deleted",
                new { r.Documentplotid });
        await _notify.NotifyAsync("Document", "Deleted", new { Id = id });

        return NoContent();
    }
}
