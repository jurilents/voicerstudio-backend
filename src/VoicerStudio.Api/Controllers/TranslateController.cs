using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Core;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/translate")]
public class TranslateController : V1Controller
{
    private readonly ITranslateService _translateService;

    public TranslateController(ITranslateService translateService)
    {
        _translateService = translateService;
    }


    [HttpGet("languages")]
    public async Task<TranslationLanguages> TranslateAsync(CancellationToken ct)
    {
        return await _translateService.GetLanguagesAsync(ct);
    }

    [HttpPost]
    public async Task<TranslationModel[]> TranslateAsync(TranslateRequest request, CancellationToken ct)
    {
        return await _translateService.TranslateAsync(request, ct);
    }
}