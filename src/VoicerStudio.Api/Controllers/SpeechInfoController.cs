using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/speech-info")]
public class SpeechInfoController : V1Controller
{
    private readonly ICognitiveService _cognitiveService;

    public SpeechInfoController(ICognitiveService cognitiveService)
    {
        _cognitiveService = cognitiveService;
    }


    [HttpGet("languages")]
    public async Task<Language[]> Languages([FromCredentialsHeader] string credentials)
    {
        return await _cognitiveService.GetLanguagesAsync(credentials);
    }
}