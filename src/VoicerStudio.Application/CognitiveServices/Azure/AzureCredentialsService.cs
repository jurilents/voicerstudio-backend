using System.Text.Json;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using NeerCore.Json;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models.Speech;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.CognitiveServices.Azure;

internal class AzureCredentialsService : ICredentialsService
{
    private record AzureCredentials(string SubscriptionKey, string Region);

    private const string Prefix = "AZURE_";

    private readonly IEncryptor _encryptor;
    private readonly AzureOptions _azure;

    public AzureCredentialsService(IEncryptor encryptor, IOptions<AzureOptions> azureOptionsAccessor)
    {
        _encryptor = encryptor;
        _azure = azureOptionsAccessor.Value;
    }


    public CognitiveServiceName ServiceName => CognitiveServiceName.Azure;

    public async Task<SecureCredentialsResult> SecureAsync(SecureCredentialsRequest request)
    {
        var cred = new AzureCredentials(request.Data["subscriptionKey"], request.Data["region"]);
        if (cred.SubscriptionKey == _azure.User.SubscriptionKey && cred.Region == _azure.User.Region)
        {
            // Replace fake creds with real ones from config
            cred = new AzureCredentials(_azure.Credentials.SubscriptionKey, _azure.Credentials.Region);
        }
        else if (!await ValidateAzureCredentialsAsync(cred))
            throw new ValidationFailedException("Credentials are invalid");

        var jsonString = JsonSerializer.Serialize(cred, JsonConventions.CamelCase);
        var encryptedCredentials = _encryptor.Encrypt(jsonString);

        return new SecureCredentialsResult(Credentials: encryptedCredentials);
    }

    public Task<IReadOnlyDictionary<string, string>> UnsecureAsync(string credentials)
    {
        if (string.IsNullOrEmpty(credentials))
            throw new ValidationFailedException("Credentials are invalid");

        if (_azure.User.ApiKeys is not null && _azure.User.ApiKeys.Contains(credentials))
        {
            return Task.FromResult<IReadOnlyDictionary<string, string>>(new Dictionary<string, string>
            {
                ["subscriptionKey"] = _azure.Credentials.SubscriptionKey,
                ["region"] = _azure.Credentials.Region,
            });
        }

        try
        {
            var decryptedCredentials = _encryptor.Decrypt(credentials);
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