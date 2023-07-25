using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.Infrastructure;

internal sealed class CredentialsService : ICredentialsService
{
    private record AzureCredentials(string SubscriptionKey, string Region);

    private record VoiceMakerCredentials(string ApiKey);

    private readonly IEncryptor _encryptor;
    private readonly AzureOptions _azure;
    private readonly VoiceMakerOptions _voiceMaker;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private HttpContext HttpContext => _httpContextAccessor.HttpContext!;

    public CredentialsService(
        IHttpContextAccessor httpContextAccessor, IEncryptor encryptor,
        IOptions<AzureOptions> azureOptionsAccessor, IOptions<VoiceMakerOptions> voiceMakerOptionsAccessor)
    {
        _encryptor = encryptor;
        _azure = azureOptionsAccessor.Value;
        _voiceMaker = voiceMakerOptionsAccessor.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<SecureCredentialsResult> SecureAsync(SecureCredentialsRequest request)
    {
        var encryptedData = request.Service switch
        {
            CognitiveServiceName.Azure      => SecureAzureCredentialsAsync(request),
            CognitiveServiceName.VoiceMaker => SecureVoiceMakerCredentialsAsync(request),
            _                               => throw new ValidationFailedException($"Invalid service '{request.Service}'"),
        };
        return new SecureCredentialsResult(Credentials: await encryptedData);
    }

    private async Task<string> SecureAzureCredentialsAsync(SecureCredentialsRequest request)
    {
        var cred = new AzureCredentials(request.Data!["SubscriptionKey"], request.Data["Region"]);
        if (cred.SubscriptionKey == _azure.User.SubscriptionKey && cred.Region == _azure.User.Region)
        {
            // Replace fake creds with real ones from config
            cred = new AzureCredentials(_azure.Credentials.SubscriptionKey, _azure.Credentials.Region);
        }
        else if (!await ValidateAzureCredentialsAsync(cred))
            throw new ValidationFailedException("Credentials are invalid");

        var jsonString = "AZURE_" + JsonSerializer.Serialize(cred);
        return _encryptor.Encrypt(jsonString);
    }

    private async Task<string> SecureVoiceMakerCredentialsAsync(SecureCredentialsRequest request)
    {
        var cred = new VoiceMakerCredentials(request.Data!["ApiKey"]);
        if (cred.ApiKey == _voiceMaker.User.ApiKey)
        {
            // Replace fake creds with real ones from config
            cred = new VoiceMakerCredentials(_voiceMaker.Credentials.ApiKey);
        }
        else if (!await ValidateVoiceMakerCredentialsAsync(cred))
            throw new ValidationFailedException("Credentials are invalid");

        var jsonString = "VMKER_" + JsonSerializer.Serialize(cred);
        return _encryptor.Encrypt(jsonString);
    }

    private static async Task<bool> ValidateAzureCredentialsAsync(AzureCredentials cred)
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

    private static async Task<bool> ValidateVoiceMakerCredentialsAsync(VoiceMakerCredentials cred)
    {
        if (string.IsNullOrWhiteSpace(cred.ApiKey))
            return false;

        // TODO: Make here a request to voicemaker to validate it's credentials

        return true;
    }
}