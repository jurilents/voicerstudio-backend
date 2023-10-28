using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Models.Speech;

namespace VoicerStudio.Application.Services;

public interface ICredentialsService
{
    CredentialsType ServiceName { get; }
    Task<SecureCredentialsResult> SecureAsync(SecureCredentialsRequest request);
    Task<IReadOnlyDictionary<string, string>> UnsecureAsync(string credentials);
}