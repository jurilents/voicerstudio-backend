using NeerCore.Exceptions;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Services;

namespace VoicerStudio.CognitiveServices;

public class CredentialsServicesProvider
{
    private readonly IEnumerable<ICredentialsService> _credentialsServices;

    public CredentialsServicesProvider(IEnumerable<ICredentialsService> credentialsServices)
    {
        _credentialsServices = credentialsServices;
    }


    public ICredentialsService GetService(CredentialsType serviceName)
    {
        var audioService = _credentialsServices.FirstOrDefault(x => x.ServiceName == serviceName);
        return audioService
            ?? throw new ValidationFailedException($"Invalid cognitive service name provided '{serviceName.ToString().ToLower()}'");
    }
}