using Microsoft.AspNetCore.Mvc;
using VoicerStudio.Api.Controllers.Core;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Models.TranslationDocs;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Api.Controllers;

[Route("/v1/translation-docs")]
public class TranslationDocsController : V1Controller
{
    private readonly ITranslationDocsParser _translationDocsParser;

    public TranslationDocsController(ITranslationDocsParser translationDocsParser)
    {
        _translationDocsParser = translationDocsParser;
    }


    [HttpGet("{fileId}")]
    public async Task<ParseTranslationDocResult> GetFile(
        [FromRoute] string fileId, [FromQuery] ParseTranslationDocQuery query, CancellationToken ct)
    {
        return await _translationDocsParser.ParseFileAsync(fileId, query, ct);
    }
}