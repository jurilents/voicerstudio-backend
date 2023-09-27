using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Application.Models.ConferenceTranslations;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/conference-translations")]
public class ConferenceTranslationsController : V1Controller
{
    private readonly IConferenceTranslationsService _ctService;

    public ConferenceTranslationsController(IConferenceTranslationsService ctService)
    {
        _ctService = ctService;
    }


    [HttpGet]
    public async Task<EventModel[]> Events([FromCredentialsHeader] string authKey, CancellationToken ct)
    {
        return await _ctService.GetEventsAsync(authKey, ct);
    }
}