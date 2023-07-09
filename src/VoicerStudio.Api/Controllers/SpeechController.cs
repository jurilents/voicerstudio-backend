using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Api.Extensions;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/speech")]
public class SpeechController : V1Controller
{
    private readonly ICognitiveService _cognitiveService;

    public SpeechController(ICognitiveService cognitiveService)
    {
        _cognitiveService = cognitiveService;
    }


    [HttpPost("single")]
    [Produces("audio/wav")]
    public async Task<IActionResult> GenerateSingle(
        [FromBody] SpeechGenerateRequest request, [FromCredentialsHeader] string credentials)
    {
        var result = await _cognitiveService.GenerateSpeechAsync(request, credentials);

        Response.AddDurationHeader(result.Duration);
        return File(result.AudioData, result.MimeType);
    }


    [HttpPost("batch")]
    [Produces("audio/wav")]
    public async Task<IActionResult> GenerateBatch(
        [FromBody] SpeechGenerateRequest[] requests, [FromCredentialsHeader] string credentials)
    {
        var result = await _cognitiveService.GenerateSpeechAsync(requests, credentials);

        Response.AddDurationHeader(result.Duration);
        return File(result.AudioData, result.MimeType);
    }
}