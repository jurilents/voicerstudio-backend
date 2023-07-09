namespace VoicerStudio.Application.Services;

public interface IEncryptor
{
    string Encrypt(string plainText);
    string Decrypt(string encryptedText);
}