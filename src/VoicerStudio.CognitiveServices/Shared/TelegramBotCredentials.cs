using System.Text.Json;
using Microsoft.CognitiveServices.Speech;
using NeerCore.Exceptions;
using NeerCore.Json;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models.Speech;
using VoicerStudio.Application.Services;
using VoicerStudio.Database.Context;
using VoicerStudio.Database.Entities;

namespace VoicerStudio.CognitiveServices.Shared;

internal class TelegramBotCredentials : ICredentialsService
{
    private record TelegramCredentials(string UserToken);

    public const string Prefix = "TGKEY_";

    private readonly AppDbContext _dbContext;
    private readonly IEncryptor _encryptor;

    public TelegramBotCredentials(AppDbContext dbContext, IEncryptor encryptor)
    {
        _dbContext = dbContext;
        _encryptor = encryptor;
    }

    public CredentialsType ServiceName => CredentialsType.AuthorizerBot;


    public async Task<SecureCredentialsResult> SecureAsync(SecureCredentialsRequest request)
    {
        var cred = new TelegramCredentials(request.Data["userToken"]);

        var token = await GetTokenFromCredentialsAsync(cred);
        if (token is null)
            throw new ValidationFailedException("Credentials are invalid");

        var jsonString = JsonSerializer.Serialize(cred, JsonConventions.CamelCase);
        var encryptedCredentials = Prefix + _encryptor.Encrypt(jsonString);

        return new SecureCredentialsResult(Credentials: encryptedCredentials);
    }

    public Task<IReadOnlyDictionary<string, string>> UnsecureAsync(string credentials)
    {
        if (string.IsNullOrEmpty(credentials) || credentials.StartsWith(Prefix))
            throw new ValidationFailedException("Credentials are invalid");

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

    private async Task<AppToken?> GetTokenFromCredentialsAsync(TelegramCredentials cred)
    {
        if (string.IsNullOrWhiteSpace(cred.UserToken))
            return null;

        var token = await _dbContext.Tokens.FindAsync(cred.UserToken);
        return token;
    }
}