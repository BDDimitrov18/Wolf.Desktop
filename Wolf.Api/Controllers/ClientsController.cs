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
public class ClientsController : ControllerBase
{
    private readonly IRepository<Client> _repository;
    private readonly INotificationService _notify;

    public ClientsController(IRepository<Client> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new ClientDto
        {
            Clientid = e.Clientid,
            Firstname = e.Firstname,
            Middlename = e.Middlename,
            Lastname = e.Lastname,
            Phone = e.Phone,
            Email = e.Email,
            Address = e.Address,
            Clientlegaltype = e.Clientlegaltype
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new ClientDto
        {
            Clientid = entity.Clientid,
            Firstname = entity.Firstname,
            Middlename = entity.Middlename,
            Lastname = entity.Lastname,
            Phone = entity.Phone,
            Email = entity.Email,
            Address = entity.Address,
            Clientlegaltype = entity.Clientlegaltype
        });
    }

    [HttpPost]
    public async Task<ActionResult<ClientDto>> Create(CreateClientDto dto)
    {
        var entity = new Client
        {
            Firstname = dto.Firstname,
            Middlename = dto.Middlename,
            Lastname = dto.Lastname,
            Phone = dto.Phone,
            Email = dto.Email,
            Address = dto.Address,
            Clientlegaltype = dto.Clientlegaltype
        };

        await _repository.AddAsync(entity);

        var result = new ClientDto
        {
            Clientid = entity.Clientid,
            Firstname = entity.Firstname,
            Middlename = entity.Middlename,
            Lastname = entity.Lastname,
            Phone = entity.Phone,
            Email = entity.Email,
            Address = entity.Address,
            Clientlegaltype = entity.Clientlegaltype
        };
        await _notify.NotifyAsync("Client", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Clientid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, ClientDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Firstname = dto.Firstname;
        entity.Middlename = dto.Middlename;
        entity.Lastname = dto.Lastname;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Address = dto.Address;
        entity.Clientlegaltype = dto.Clientlegaltype;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("Client", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("Client", "Deleted", new { Id = id });
        return NoContent();
    }
}
