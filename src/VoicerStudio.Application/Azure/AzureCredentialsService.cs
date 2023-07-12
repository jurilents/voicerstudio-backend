using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.Azure;

internal sealed class AzureCredentialsService : ICredentialsService
{
    private record AzureCredentials(string SubscriptionKey, string Region);

    private readonly IEncryptor _encryptor;
    private readonly AzureOptions _azure;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private HttpContext HttpContext => _httpContextAccessor.HttpContext!;

    public AzureCredentialsService(
        IHttpContextAccessor httpContextAccessor, IEncryptor encryptor, IOptions<AzureOptions> credentialsOptionsAccessor)
    {
        _encryptor = encryptor;
        _azure = credentialsOptionsAccessor.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SecureCredentialsResult> SecureAsync(SecureCredentialsRequest request)
    {
        var cred = new AzureCredentials(request.Data!["SubscriptionKey"], request.Data["Region"]);
        if (IsDefaultUser(cred))
        {
            // Replace model creds with default ones from config
            cred = new AzureCredentials(_azure.Credentials.SubscriptionKey, _azure.Credentials.Region);
        }
        else if (!await ValidateCredentialsAsync(cred))
            throw new ValidationFailedException("Credentials are invalid");

        var jsonString = "AZURE_" + JsonSerializer.Serialize(cred);
        var encryptedData = _encryptor.Encrypt(jsonString);
        return new SecureCredentialsResult(Credentials: encryptedData);
    }

    private bool IsDefaultUser(AzureCredentials credentials)
    {
        return credentials.SubscriptionKey == _azure.User.SubscriptionKey
            && credentials.Region == _azure.User.Region;
    }

    private static async Task<bool> ValidateCredentialsAsync(AzureCredentials cred)
    {
        if (string.IsNullOrWhiteSpace(cred.SubscriptionKey) || string.IsNullOrWhiteSpace(cred.Region))
            return false;

        var testConfig = SpeechConfig.FromSubscription(cred.SubscriptionKey, cred.Region);
        using var synthesizer = new SpeechSynthesizer(testConfig);
        var result = await synthesizer.GetVoicesAsync();
        if (result.Reason is ResultReason.Canceled || result.Voices.Count == 0)
            return false;

        return true;
    }
}