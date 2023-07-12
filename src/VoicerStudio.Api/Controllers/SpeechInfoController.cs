using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/speech-info")]
public class SpeechInfoController : V1Controller
{
    private readonly IEnumerable<ICognitiveService> _cognitiveServices;

    public SpeechInfoController(IEnumerable<ICognitiveService> cognitiveServices)
    {
        _cognitiveServices = cognitiveServices;
    }


    [HttpGet("languages")]
    public async Task<Language[]> Languages(
        [FromQuery] CognitiveServiceName service, [FromCredentialsHeader] string credentials)
    {
        var cognitiveService = _cognitiveServices.First(x => x.ServiceName == service);
        return await cognitiveService.GetLanguagesAsync(credentials);
    }
}