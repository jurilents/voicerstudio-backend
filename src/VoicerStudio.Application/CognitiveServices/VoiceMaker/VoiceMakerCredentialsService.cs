using System.Text.Json;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using NeerCore.Json;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models.Speech;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.CognitiveServices.VoiceMaker;

internal class VoiceMakerCredentialsService : ICredentialsService
{
    private record VoiceMakerCredentials(string ApiKey);

    private const string Prefix = "VMAKER_";

    private readonly IEncryptor _encryptor;
    private readonly VoiceMakerOptions _options;

    public VoiceMakerCredentialsService(IOptions<VoiceMakerOptions> voiceMakerOptionsAccessor, IEncryptor encryptor)
    {
        _encryptor = encryptor;
        _options = voiceMakerOptionsAccessor.Value;
    }


    public CognitiveServiceName ServiceName => CognitiveServiceName.VoiceMaker;

    public async Task<SecureCredentialsResult> SecureAsync(SecureCredentialsRequest request)
    {
        var cred = new VoiceMakerCredentials(request.Data["apiKey"]);
        if (cred.ApiKey == _options.User.ApiKey)
        {
            // Replace fake creds with real ones from config
            cred = new VoiceMakerCredentials(_options.Credentials.ApiKey);
        }
        else if (!await ValidateVoiceMakerCredentialsAsync(cred))
            throw new ValidationFailedException("Credentials are invalid");

        var jsonString = JsonSerializer.Serialize(cred, JsonConventions.CamelCase);
        var encryptedCredentials = Prefix + _encryptor.Encrypt(jsonString);

        return new SecureCredentialsResult(Credentials: encryptedCredentials);
    }

    public Task<IReadOnlyDictionary<string, string>> UnsecureAsync(string credentials)
    {
        if (string.IsNullOrEmpty(credentials))
            throw new ValidationFailedException("Credentials are invalid");

        try
        {
            var decryptedCredentials = _encryptor.Decrypt(credentials)[Prefix.Length..];
            var cred = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedCredentials)!;

            return Task.FromResult<IReadOnlyDictionary<string, string>>(cred);
        }
        catch
        {
            throw new ValidationFailedException("Credentials are invalid");
        }
    }

    private static async Task<bool> ValidateVoiceMakerCredentialsAsync(VoiceMakerCredentials cred)
    {
        if (string.IsNullOrWhiteSpace(cred.ApiKey))
            return false;

        // TODO: Make here a request to voicemaker to validate it's credentials

        return true;
    }
}