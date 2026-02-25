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
public class PlotsController : ControllerBase
{
    private readonly IRepository<Plot> _repository;
    private readonly INotificationService _notify;

    public PlotsController(IRepository<Plot> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PlotDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new PlotDto
        {
            Plotid = e.Plotid,
            Plotnumber = e.Plotnumber,
            Regulatedplotnumber = e.Regulatedplotnumber,
            Neighborhood = e.Neighborhood,
            City = e.City,
            Municipality = e.Municipality,
            Street = e.Street,
            Streetnumber = e.Streetnumber,
            Designation = e.Designation,
            Locality = e.Locality
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PlotDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new PlotDto
        {
            Plotid = entity.Plotid,
            Plotnumber = entity.Plotnumber,
            Regulatedplotnumber = entity.Regulatedplotnumber,
            Neighborhood = entity.Neighborhood,
            City = entity.City,
            Municipality = entity.Municipality,
            Street = entity.Street,
            Streetnumber = entity.Streetnumber,
            Designation = entity.Designation,
            Locality = entity.Locality
        });
    }

    [HttpPost]
    public async Task<ActionResult<PlotDto>> Create(CreatePlotDto dto)
    {
        var entity = new Plot
        {
            Plotnumber = dto.Plotnumber,
            Regulatedplotnumber = dto.Regulatedplotnumber,
            Neighborhood = dto.Neighborhood,
            City = dto.City,
            Municipality = dto.Municipality,
            Street = dto.Street,
            Streetnumber = dto.Streetnumber,
            Designation = dto.Designation,
            Locality = dto.Locality
        };

        await _repository.AddAsync(entity);

        var result = new PlotDto
        {
            Plotid = entity.Plotid,
            Plotnumber = entity.Plotnumber,
            Regulatedplotnumber = entity.Regulatedplotnumber,
            Neighborhood = entity.Neighborhood,
            City = entity.City,
            Municipality = entity.Municipality,
            Street = entity.Street,
            Streetnumber = entity.Streetnumber,
            Designation = entity.Designation,
            Locality = entity.Locality
        };
        await _notify.NotifyAsync("Plot", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Plotid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, PlotDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Plotnumber = dto.Plotnumber;
        entity.Regulatedplotnumber = dto.Regulatedplotnumber;
        entity.Neighborhood = dto.Neighborhood;
        entity.City = dto.City;
        entity.Municipality = dto.Municipality;
        entity.Street = dto.Street;
        entity.Streetnumber = dto.Streetnumber;
        entity.Designation = dto.Designation;
        entity.Locality = dto.Locality;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("Plot", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("Plot", "Deleted", new { Id = id });
        return NoContent();
    }
}
