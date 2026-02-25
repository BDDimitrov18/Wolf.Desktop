using Microsoft.AspNetCore.Mvc;
using Wolf.Api.Repositories;
using Wolf.Dtos;
using FileEntity = DataLayer.Entities.File;
using Microsoft.AspNetCore.Authorization;

namespace Wolf.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IRepository<FileEntity> _repository;

    public FilesController(IRepository<FileEntity> repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FileDto>>> GetAll()
    {
        var entities = await _repository.GetAllAsync();
        var dtos = entities.Select(e => new FileDto
        {
            Fileid = e.Fileid,
            Filename = e.Filename,
            Filepath = e.Filepath,
            Uploadedat = e.Uploadedat
        });
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<FileDto>> GetById(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        return Ok(new FileDto
        {
            Fileid = entity.Fileid,
            Filename = entity.Filename,
            Filepath = entity.Filepath,
            Uploadedat = entity.Uploadedat
        });
    }

    [HttpPost]
    public async Task<ActionResult<FileDto>> Create(CreateFileDto dto)
    {
        var entity = new FileEntity
        {
            Filename = dto.Filename,
            Filepath = dto.Filepath,
            Uploadedat = dto.Uploadedat
        };

        await _repository.AddAsync(entity);

        var result = new FileDto
        {
            Fileid = entity.Fileid,
            Filename = entity.Filename,
            Filepath = entity.Filepath,
            Uploadedat = entity.Uploadedat
        };
        return CreatedAtAction(nameof(GetById), new { id = entity.Fileid }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, FileDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        entity.Filename = dto.Filename;
        entity.Filepath = dto.Filepath;
        entity.Uploadedat = dto.Uploadedat;

        await _repository.UpdateAsync(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return NotFound();

        await _repository.DeleteAsync(entity);
        return NoContent();
    }
}
