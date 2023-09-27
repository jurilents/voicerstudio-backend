using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Application.Repositories;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/storage")]
public class StorageController : V1Controller
{
    private readonly IEventRepository _eventRepository;

    public StorageController(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }


    [HttpGet("events/{slug}/timing")]
    public async Task<object> Get(string slug, CancellationToken ct)
    {
        return await _eventRepository.GetTimingAsync(slug, ct);
    }

    [HttpPost("events/{slug}/timing")]
    public async Task Set(string slug, [FromBody] object timing, CancellationToken ct)
    {
        await _eventRepository.SetTimingAsync(slug, timing, ct);
    }
}