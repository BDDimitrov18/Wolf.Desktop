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
public class ClientRequestrelashionshipsController : ControllerBase
{
    private readonly IRepository<ClientRequestrelashionship> _repository;
    private readonly INotificationService _notify;

    public ClientRequestrelashionshipsController(IRepository<ClientRequestrelashionship> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientRequestrelashionshipDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new ClientRequestrelashionshipDto
        {
            Requestid = e.Requestid,
            Clientid = e.Clientid,
            Ownershiptype = e.Ownershiptype
        });
        return Ok(dtos);
    }

    [HttpGet("{requestId}/{clientId}")]
    public async Task<ActionResult<ClientRequestrelashionshipDto>> GetById(int requestId, int clientId)
    {
        var entity = await _repository.GetByIdAsync(requestId, clientId);
        if (entity is null) return NotFound();

        return Ok(new ClientRequestrelashionshipDto
        {
            Requestid = entity.Requestid,
            Clientid = entity.Clientid,
            Ownershiptype = entity.Ownershiptype
        });
    }

    [HttpPost]
    public async Task<ActionResult<ClientRequestrelashionshipDto>> Create(CreateClientRequestrelashionshipDto dto)
    {
        var entity = new ClientRequestrelashionship
        {
            Requestid = dto.Requestid,
            Clientid = dto.Clientid,
            Ownershiptype = dto.Ownershiptype
        };

        await _repository.AddAsync(entity);

        var result = new ClientRequestrelashionshipDto
        {
            Requestid = entity.Requestid,
            Clientid = entity.Clientid,
            Ownershiptype = entity.Ownershiptype
        };
        await _notify.NotifyAsync("ClientRequestRel", "Created", result);
        return CreatedAtAction(nameof(GetById),
            new { requestId = entity.Requestid, clientId = entity.Clientid },
            result);
    }

    [HttpPut("{requestId}/{clientId}")]
    public async Task<IActionResult> Update(int requestId, int clientId, ClientRequestrelashionshipDto dto)
    {
        var entity = await _repository.GetByIdAsync(requestId, clientId);
        if (entity is null) return NotFound();

        entity.Ownershiptype = dto.Ownershiptype;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("ClientRequestRel", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{requestId}/{clientId}")]
    public async Task<IActionResult> Delete(int requestId, int clientId)
    {
        var entity = await _repository.GetByIdAsync(requestId, clientId);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("ClientRequestRel", "Deleted", new { Requestid = requestId, Clientid = clientId });
        return NoContent();
    }
}
