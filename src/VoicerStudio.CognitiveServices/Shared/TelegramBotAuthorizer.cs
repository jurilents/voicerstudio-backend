using System.Text.Json;
using VoicerStudio.Application.Services;

namespace VoicerStudio.CognitiveServices.Shared;

public class TelegramBotAuthorizer
{
    private const string Prefix = TelegramBotCredentials.Prefix;

    private readonly IEncryptor _encryptor;

    public TelegramBotAuthorizer(IEncryptor encryptor)
    {
        _encryptor = encryptor;
    }


    public bool IsValidTelegramBotCredentials(string credentials)
    {
        if (!credentials.StartsWith(Prefix))
            return false;

        try
        {
            var decryptedCredentials = _encryptor.Decrypt(credentials[Prefix.Length..]);
            var cred = JsonSerializer.Deserialize<Dictionary<string, string>>(decryptedCredentials)!;

            return !string.IsNullOrEmpty(cred["userToken"]);
        }
        catch
        {
            return false;
        }
    }
}