using VoicerStudio.Application.Models;

namespace VoicerStudio.Application.Services;

public interface ICredentialsService
{
    Task<SecureCredentialsResult> SecureAsync(SecureCredentialsRequest request);
}