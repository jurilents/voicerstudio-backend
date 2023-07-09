using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Repositories;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/subtitle")]
public class SubtitleController : V1Controller
{
    private readonly ISubtitleRepository _subtitleRepository;

    public SubtitleController(ISubtitleRepository subtitleRepository)
    {
        _subtitleRepository = subtitleRepository;
    }


    [HttpGet("{id:guid}")]
    public async Task<Subtitle> Get(Guid id, string sheet)
    {
        return await _subtitleRepository.GetByIdAsync(id, sheet);
    }

    [HttpGet]
    public async Task<IEnumerable<Subtitle>> GetAll(string sheet)
    {
        return await _subtitleRepository.GetAllAsync(sheet);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Subtitle[] subs, string sheet)
    {
        await _subtitleRepository.UpdateAsync(subs, sheet);
        return NoContent();
    }
}