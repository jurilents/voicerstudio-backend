using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Application.Models.Speech;
using VoicerStudio.CognitiveServices;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/credentials")]
public class CredentialsController : V1Controller
{
    private readonly CredentialsServicesProvider _credentialsServices;

    public CredentialsController(CredentialsServicesProvider credentialsServices)
    {
        _credentialsServices = credentialsServices;
    }


    [HttpPost("secure")]
    public async Task<SecureCredentialsResult> Secure([FromBody] SecureCredentialsRequest request)
    {
        var credentialsService = _credentialsServices.GetService(request.Service);
        return await credentialsService.SecureAsync(request);
    }
}