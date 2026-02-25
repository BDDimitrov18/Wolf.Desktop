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
public class EmployeesController : ControllerBase
{
    private readonly IRepository<Employee> _repository;
    private readonly INotificationService _notify;

    public EmployeesController(IRepository<Employee> repository, INotificationService notify)
    {
        _repository = repository;
        _notify = notify;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new EmployeeDto
        {
            Employeeid = e.Employeeid,
            Firstname = e.Firstname,
            Secondname = e.Secondname,
            Lastname = e.Lastname,
            Phone = e.Phone,
            Email = e.Email,
            Outsider = e.Outsider
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new EmployeeDto
        {
            Employeeid = entity.Employeeid,
            Firstname = entity.Firstname,
            Secondname = entity.Secondname,
            Lastname = entity.Lastname,
            Phone = entity.Phone,
            Email = entity.Email,
            Outsider = entity.Outsider
        });
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(CreateEmployeeDto dto)
    {
        var entity = new Employee
        {
            Firstname = dto.Firstname,
            Secondname = dto.Secondname,
            Lastname = dto.Lastname,
            Phone = dto.Phone,
            Email = dto.Email,
            Outsider = dto.Outsider
        };

        await _repository.AddAsync(entity);

        var result = new EmployeeDto
        {
            Employeeid = entity.Employeeid,
            Firstname = entity.Firstname,
            Secondname = entity.Secondname,
            Lastname = entity.Lastname,
            Phone = entity.Phone,
            Email = entity.Email,
            Outsider = entity.Outsider
        };
        await _notify.NotifyAsync("Employee", "Created", result);
        return CreatedAtAction(nameof(GetById), new { id = entity.Employeeid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, EmployeeDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Firstname = dto.Firstname;
        entity.Secondname = dto.Secondname;
        entity.Lastname = dto.Lastname;
        entity.Phone = dto.Phone;
        entity.Email = dto.Email;
        entity.Outsider = dto.Outsider;

        await _repository.UpdateAsync(entity);
        await _notify.NotifyAsync("Employee", "Updated", dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        await _notify.NotifyAsync("Employee", "Deleted", new { Id = id });
        return NoContent();
    }
}
