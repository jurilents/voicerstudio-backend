using FluentValidation;
using VoicerStudio.Application.Enums;

namespace VoicerStudio.Application.Models;

public class SecureCredentialsRequest
{
    public CognitiveServiceName Service { get; set; }
    public Dictionary<string, string>? Data { get; set; }
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
                    data.ContainsKey("SubscriptionKey") && data.ContainsKey("Region"))
                .WithMessage("Data properties are required for Azure service: 'SubscriptionKey' and 'Region'");
        });

        When(o => o.Service == CognitiveServiceName.VoiceMaker, () =>
        {
            RuleFor(o => o.Data).Must(data =>
                    data.ContainsKey("ApiKey"))
                .WithMessage("Data properties are required for VoiceMaker service: 'ApiKey'");
        });
    }
}