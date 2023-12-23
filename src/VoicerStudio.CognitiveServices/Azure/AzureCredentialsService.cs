using System.Text.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using NeerCore.Json;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models.Speech;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;
using VoicerStudio.CognitiveServices.Shared;

namespace VoicerStudio.CognitiveServices.Azure;

internal class AzureCredentialsService : ICredentialsService
{
    private record AzureCredentials(string SubscriptionKey, string Region);

    private const string Prefix = "AZURE_";

    private readonly IEncryptor _encryptor;
    private readonly TelegramBotAuthorizer _telegramBotAuthorizer;
    private readonly AzureOptions _azure;

    public AzureCredentialsService(
        IEncryptor encryptor, TelegramBotAuthorizer telegramBotAuthorizer, IOptions<AzureOptions> azureOptionsAccessor)
    {
        _encryptor = encryptor;
        _telegramBotAuthorizer = telegramBotAuthorizer;
        _azure = azureOptionsAccessor.Value;
    }

    public CredentialsType ServiceName => CredentialsType.Azure;


    public async Task<SecureCredentialsResult> SecureAsync(SecureCredentialsRequest request)
    {
        var cred = new AzureCredentials(request.Data["subscriptionKey"], request.Data["region"]);
        if (!await ValidateAzureCredentialsAsync(cred))
            throw new ValidationFailedException("Credentials are invalid");

        var jsonString = JsonSerializer.Serialize(cred, JsonConventions.CamelCase);
        var encryptedCredentials = Prefix + _encryptor.Encrypt(jsonString);

        return new SecureCredentialsResult(Credentials: encryptedCredentials);
    }

    public Task<IReadOnlyDictionary<string, string>> UnsecureAsync(string credentials)
    {
        if (string.IsNullOrEmpty(credentials))
            throw new ValidationFailedException("Credentials are invalid");

        if (_telegramBotAuthorizer.IsValidTelegramBotCredentials(credentials))
        {
            return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>
            {
                ["subscriptionKey"] = _azure.SubscriptionKey,
                ["region"] = _azure.Region,
            });
        }

        try
        {
            var decryptedCredentials = _encryptor.Decrypt(credentials[Prefix.Length..]);
            var cred = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedCredentials)!;

            return Task.FromResult<IReadOnlyDictionary<string, string>>(cred);
        }
        catch
        {
            throw new ValidationFailedException("Credentials are invalid");
        }
    }

    private static async Task<bool> ValidateAzureCredentialsAsync(AzureCredentials cred)
    {
        if (string.IsNullOrWhiteSpace(cred.SubscriptionKey) || string.IsNullOrWhiteSpace(cred.Region))
            return false;

        var testConfig = SpeechConfig.FromSubscription(cred.SubscriptionKey, cred.Region);
        using var synthesizer = new SpeechSynthesizer(testConfig);
        var result = await synthesizer.GetVoicesAsync();
        return result.Reason is not ResultReason.Canceled && result.Voices.Count != 0;
    }
}