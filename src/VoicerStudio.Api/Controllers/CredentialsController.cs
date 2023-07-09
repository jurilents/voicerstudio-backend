using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/credentials")]
public class CredentialsController : V1Controller
{
    private readonly ICredentialsService _credentialsService;

    public CredentialsController(ICredentialsService credentialsService)
    {
        _credentialsService = credentialsService;
    }


    [HttpPost("secure")]
    public async Task<SecureCredentialsResult> Secure([FromBody] SecureCredentialsRequest request)
    {
        return await _credentialsService.SecureAsync(request);
    }
}