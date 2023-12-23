using FluentValidation;

namespace VoicerStudio.Application.Models;

public class TranslateRequest
{
    public required string[] Texts { get; set; }
    public required string SourceLang { get; set; }
    public required string TargetLang { get; set; }
}

public class SpeechGenerateRequestValidator : AbstractValidator<TranslateRequest>
{
    public SpeechGenerateRequestValidator()
    {
        RuleFor(o => o.Texts).Must(o => o.Length > 0 && o.Any(x => !string.IsNullOrWhiteSpace(x)))
            .WithMessage("No text to translate");
        RuleFor(o => o.SourceLang).NotEmpty();
        RuleFor(o => o.TargetLang).NotEmpty();
    }
}