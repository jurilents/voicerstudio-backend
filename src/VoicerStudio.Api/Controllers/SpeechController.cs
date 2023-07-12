using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Api.Extensions;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/speech")]
public class SpeechController : V1Controller
{
    private readonly IEnumerable<ICognitiveService> _cognitiveServices;

    public SpeechController(IEnumerable<ICognitiveService> cognitiveServices)
    {
        _cognitiveServices = cognitiveServices;
    }


    [HttpPost("single")]
    [Produces("audio/wav")]
    public async Task<IActionResult> GenerateSingle(
        [FromBody] SpeechGenerateRequest request, [FromCredentialsHeader] string credentials)
    {
        var service = _cognitiveServices.First(x => x.ServiceName == request.Service);
        var result = await service.GenerateSpeechAsync(request, credentials);

        Response.AddDurationHeader(result.OutputDuration, result.InputDuration);
        return File(result.AudioData, result.MimeType);
    }


    [HttpPost("batch")]
    [Produces("audio/wav")]
    public async Task<IActionResult> GenerateBatch(
        [FromBody] SpeechGenerateRequest[] requests, [FromCredentialsHeader] string credentials)
    {
        var serviceName = requests.First().Service;
        var service = _cognitiveServices.First(x => x.ServiceName == serviceName);
        var result = await service.GenerateSpeechAsync(requests, credentials);

        Response.AddDurationHeader(result.OutputDuration, result.InputDuration);
        return File(result.AudioData, result.MimeType);
    }
}