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
public class InvoicesController : ControllerBase
{
    private readonly IRepository<Invoice> _repository;
    private readonly INotificationService _notify;

    public InvoicesController(IRepository<Invoice> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new InvoiceDto
        {
            Invoiceid = e.Invoiceid,
            Requestid = e.Requestid,
            Sum = e.Sum,
            Number = e.Number
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InvoiceDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new InvoiceDto
        {
            Invoiceid = entity.Invoiceid,
            Requestid = entity.Requestid,
            Sum = entity.Sum,
            Number = entity.Number
        });
    }

    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create(CreateInvoiceDto dto)
    {
        var entity = new Invoice
        {
            Requestid = dto.Requestid,
            Sum = dto.Sum,
            Number = dto.Number
        };

        await _repository.AddAsync(entity);

        var result = new InvoiceDto
        {
            Invoiceid = entity.Invoiceid,
            Requestid = entity.Requestid,
            Sum = entity.Sum,
            Number = entity.Number
        };
        await _notify.NotifyAsync("Invoice", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Invoiceid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, InvoiceDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Requestid = dto.Requestid;
        entity.Sum = dto.Sum;
        entity.Number = dto.Number;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("Invoice", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("Invoice", "Deleted", new { Id = id });
        return NoContent();
    }
}
