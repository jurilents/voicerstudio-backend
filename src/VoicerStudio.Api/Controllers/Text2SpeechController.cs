using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Api.Shared.Extensions;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Models.Speech;
using VoicerStudio.CognitiveServices;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/text2speech")]
public class Text2SpeechController : V1Controller
{
    private readonly CognitiveServicesProvider _cognitiveServices;

    public Text2SpeechController(CognitiveServicesProvider cognitiveServices)
    {
        _cognitiveServices = cognitiveServices;
    }


    [HttpGet("languages")]
    public async Task<LanguageWithVoices[]> Languages(
        [FromQuery] CognitiveServiceName service, [FromCredentialsHeader] string credentials)
    {
        var cognitiveService = _cognitiveServices.GetService(service);
        return await cognitiveService.GetLanguagesAsync(credentials);
    }

    [HttpPost("single")]
    [Produces("audio/wav")] // or audio/mp3
    public async Task<IActionResult> GenerateSingle(
        [FromBody] SpeechGenerateRequest request, [FromCredentialsHeader] string credentials)
    {
        var cognitiveService = _cognitiveServices.GetService(request.Service);
        var result = await cognitiveService.GenerateSpeechAsync(request, credentials);

        Response.AddDurationHeader(result.OutputDuration, result.InputDuration);
        return File(result.AudioData, result.MimeType);
    }

    [HttpPost("batch")]
    [Produces("audio/wav")] // or audio/mp3
    public async Task<IActionResult> GenerateBatch(
        [FromBody] SpeechGenerateRequest[] requests, [FromCredentialsHeader] string credentials)
    {
        var serviceName = requests.First().Service;
        var cognitiveService = _cognitiveServices.GetService(serviceName);
        var result = await cognitiveService.GenerateSpeechAsync(requests, credentials);

        Response.AddDurationHeader(result.OutputDuration, result.InputDuration);
        return File(result.AudioData, result.MimeType);
    }
}