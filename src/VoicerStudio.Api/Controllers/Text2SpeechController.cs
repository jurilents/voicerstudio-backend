using Microsoft.AspNetCore.Mvc;
using NeerCore.Exceptions;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Api.Extensions;
using VoicerStudio.Application.CognitiveServices;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Models.Speech;

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
    public async Task<Language[]> Languages(
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