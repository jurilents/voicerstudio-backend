using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Repositories;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/speakers")]
public class SpeakersController : V1Controller
{
    private readonly ISpeakerRepository _speakerRepository;

    public SpeakersController(ISpeakerRepository speakerRepository)
    {
        _speakerRepository = speakerRepository;
    }


    [HttpGet("{id:int}")]
    public async Task<Speaker> Get(int id, string sheet)
    {
        return await _speakerRepository.GetByIdAsync(id, sheet);
    }

    [HttpGet]
    public async Task<IEnumerable<Speaker>> GetAll(string sheet)
    {
        return await _speakerRepository.GetAllAsync(sheet);
    }

    [HttpPut]
    public async Task<IActionResult> Put([FromBody] Speaker[] speakers, string sheet)
    {
        await _speakerRepository.UpdateAsync(speakers, sheet);
        return NoContent();
    }
}