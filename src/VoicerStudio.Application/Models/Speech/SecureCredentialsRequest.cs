using FluentValidation;
using VoicerStudio.Application.Enums;

namespace VoicerStudio.Application.Models.Speech;

public class SecureCredentialsRequest
{
    public CredentialsType Service { get; set; }
    public Dictionary<string, string> Data { get; set; } = null!;
}

public class SecureCredentialsRequestValidator : AbstractValidator<SecureCredentialsRequest>
{
    public SecureCredentialsRequestValidator()
    {
        RuleFor(o => o.Service).IsInEnum();
        RuleFor(o => o.Data).NotEmpty();

        When(o => o.Service == CredentialsType.AuthorizerBot, () =>
        {
            RuleFor(o => o.Data).Must(data =>
                    data.ContainsKey("userToken"))
                .WithMessage("Data properties are required for custom telegram bot authorization: 'userToken'");
        });

        When(o => o.Service == CredentialsType.Azure, () =>
        {
            RuleFor(o => o.Data).Must(data =>
                    data.ContainsKey("subscriptionKey") && data.ContainsKey("region"))
                .WithMessage("Data properties are required for Azure service: 'subscriptionKey' and 'region'");
        });

        When(o => o.Service == CredentialsType.VoiceMaker, () =>
        {
            RuleFor(o => o.Data).Must(data =>
                    data.ContainsKey("apiKey"))
                .WithMessage("Data properties are required for VoiceMaker service: 'apiKey'");
        });
    }
}