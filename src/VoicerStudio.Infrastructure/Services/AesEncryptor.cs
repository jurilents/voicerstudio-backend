using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;
using VoicerStudio.Application.Models;
using VoicerStudio.Application.Options;
using VoicerStudio.Application.Services;

namespace VoicerStudio.Infrastructure.Services;

internal sealed class AesEncryptor : IEncryptor
{
    private const string SecretsFilePath = "secret.aes";

    private readonly ILogger<AesEncryptor> _logger;
    private readonly AesSettings _settings;

    public AesEncryptor(ILogger<AesEncryptor> logger)
    {
        _logger = logger;
        _settings = InitAesOptions();
    }

    public string Encrypt(string plainText)
    {
        var cipher = CreateCipher(_settings.Key);
        cipher.IV = Convert.FromBase64String(_settings.IV);

        var cryptTransform = cipher.CreateEncryptor();
        var plaintext = Encoding.UTF8.GetBytes(plainText);
        var cipherText = cryptTransform.TransformFinalBlock(plaintext, 0, plaintext.Length);

        return Convert.ToBase64String(cipherText);
    }

    public string Decrypt(string encryptedText)
    {
        var cipher = CreateCipher(_settings.Key);
        cipher.IV = Convert.FromBase64String(_settings.IV);

        var cryptTransform = cipher.CreateDecryptor();
        var encryptedBytes = Convert.FromBase64String(encryptedText);
        var plainBytes = cryptTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(plainBytes);
    }

    private AesSettings InitAesOptions()
    {
        if (File.Exists(SecretsFilePath))
        {
            try
            {
                // Try to load secret keys
                var fileData = File.ReadAllBytes(SecretsFilePath);
                var fileContent = Encoding.Unicode.GetString(fileData).Split('\n');

                return new AesSettings
                {
                    Key = fileContent[0],
                    IV = fileContent[1],
                };
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occured while trying to load secrets file");
            }
        }

        // If smth goes wrong, don't worry and generate new keys
        var key = Convert.ToBase64String(GenerateRandomBytes(32)); // 256
        var cipher = CreateCipher(key);
        var ivBase64 = Convert.ToBase64String(cipher.IV);
        var fileBytes = Encoding.Unicode.GetBytes($"{key}\n{ivBase64}");
        File.WriteAllBytes(SecretsFilePath, fileBytes);

        return new AesSettings
        {
            Key = key,
            IV = ivBase64,
        };
    }

    private static Aes CreateCipher(string keyBase64)
    {
        // Default values: Keysize 256, Padding PKC27
        var cipher = Aes.Create();
        cipher.Mode = CipherMode.CBC; // Ensure the integrity of the ciphertext if using CBC

        cipher.Padding = PaddingMode.ISO10126;
        cipher.Key = Convert.FromBase64String(keyBase64);
        return cipher;
    }

    private static byte[] GenerateRandomBytes(int length)
    {
        var byteArray = new byte[length];
        RandomNumberGenerator.Fill(byteArray);
        return byteArray;
    }
}