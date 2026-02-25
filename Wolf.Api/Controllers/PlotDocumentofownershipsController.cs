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
public class PlotDocumentofownershipsController : ControllerBase
{
    private readonly IRepository<PlotDocumentofownership> _repository;
    private readonly INotificationService _notify;
    private readonly WolfDbContext _context;

    public PlotDocumentofownershipsController(IRepository<PlotDocumentofownership> repository,
        INotificationService notify, WolfDbContext context)
    {
        _repository = repository;
        _notify = notify;
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlotDocumentofownershipDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new PlotDocumentofownershipDto
        {
            Documentplotid = e.Documentplotid,
            Plotid = e.Plotid,
            Documentofownershipid = e.Documentofownershipid
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlotDocumentofownershipDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new PlotDocumentofownershipDto
        {
            Documentplotid = entity.Documentplotid,
            Plotid = entity.Plotid,
            Documentofownershipid = entity.Documentofownershipid
        });
    }

    [HttpPost]
    public async Task<ActionResult<PlotDocumentofownershipDto>> Create(CreatePlotDocumentofownershipDto dto)
    {
        var entity = new PlotDocumentofownership
        {
            Plotid = dto.Plotid,
            Documentofownershipid = dto.Documentofownershipid
        };

        await _repository.AddAsync(entity);

        var result = new PlotDocumentofownershipDto
        {
            Documentplotid = entity.Documentplotid,
            Plotid = entity.Plotid,
            Documentofownershipid = entity.Documentofownershipid
        };
        await _notify.NotifyAsync("PlotDocumentRel", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Documentplotid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PlotDocumentofownershipDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Plotid = dto.Plotid;
        entity.Documentofownershipid = dto.Documentofownershipid;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("PlotDocumentRel", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _context.PlotDocumentofownerships.FindAsync(id);
        if (entity is null) return NotFound();

        // Cascade-delete three-way records referencing this plot-document link
        var threeWayRels = await _context.DocumentplotDocumentowenerrelashionships
            .Where(r => r.Documentplotid == id).ToListAsync();
        _context.DocumentplotDocumentowenerrelashionships.RemoveRange(threeWayRels);
        _context.PlotDocumentofownerships.Remove(entity);
        await _context.SaveChangesAsync();

        foreach (var r in threeWayRels)
            await _notify.NotifyAsync("DocPlotDocOwnerRel", "Deleted", new { r.Id });
        await _notify.NotifyAsync("PlotDocumentRel", "Deleted",
            new { Documentplotid = id });
        return NoContent();
    }
}
