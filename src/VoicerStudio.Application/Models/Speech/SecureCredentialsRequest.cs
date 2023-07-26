using System.Text.Json.Serialization;
using FluentValidation;
using NeerCore.Exceptions;
using VoicerStudio.Application.Enums;

namespace VoicerStudio.Application.Models.Speech;

public class SecureCredentialsRequest
{
    public CognitiveServiceName Service { get; set; }
    public Dictionary<string, string> Data { get; set; } = null!;

    // [JsonIgnore] public string VoiceMakerApiKey => Data.TryGetValue("apiKey", out var value)
    //     ? value
    //     : throw new ValidationFailedException("VoiceMaker service required property 'apiKey' does not provided");
}

public class SecureCredentialsRequestValidator : AbstractValidator<SecureCredentialsRequest>
{
    public SecureCredentialsRequestValidator()
    {
        RuleFor(o => o.Service).IsInEnum();
        RuleFor(o => o.Data).NotEmpty();

        When(o => o.Service == CognitiveServiceName.Azure, () =>
        {
            RuleFor(o => o.Data).Must(data =>
                    data.ContainsKey("subscriptionKey") && data.ContainsKey("region"))
                .WithMessage("Data properties are required for Azure service: 'subscriptionKey' and 'region'");
        });

        When(o => o.Service == CognitiveServiceName.VoiceMaker, () =>
        {
            RuleFor(o => o.Data).Must(data =>
                    data.ContainsKey("apiKey"))
                .WithMessage("Data properties are required for VoiceMaker service: 'apiKey'");
        });
    }
}