using System.Text.Json;
using Microsoft.Extensions.Options;
using NeerCore.Exceptions;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models.Speech;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Application.CognitiveServices.VoiceMaker;

internal class VoiceMakerCredentialsService : ICredentialsService
{
    private record VoiceMakerCredentials(string ApiKey);


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

        var jsonString = "VMAKER_" + JsonSerializer.Serialize(cred);
        var encryptedCredentials = _encryptor.Encrypt(jsonString);

        return new SecureCredentialsResult(Credentials: encryptedCredentials);
    }

    private static async Task<bool> ValidateVoiceMakerCredentialsAsync(VoiceMakerCredentials cred)
    {
        if (string.IsNullOrWhiteSpace(cred.ApiKey))
            return false;

        // TODO: Make here a request to voicemaker to validate it's credentials

        return true;
    }
}