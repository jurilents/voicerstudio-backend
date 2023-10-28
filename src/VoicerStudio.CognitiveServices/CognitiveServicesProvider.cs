using NeerCore.Exceptions;
using VoicerStudio.Application.Enums;
using VoicerStudio.Application.Services;

namespace VoicerStudio.CognitiveServices;

public sealed class CognitiveServicesProvider
{
    private readonly IEnumerable<ICognitiveService> _cognitiveServices;

    public CognitiveServicesProvider(IEnumerable<ICognitiveService> cognitiveServices)
    {
        _cognitiveServices = cognitiveServices;
    }


    public ICognitiveService GetService(CognitiveServiceName serviceName)
    {
        var audioService = _cognitiveServices.FirstOrDefault(x => x.ServiceName == serviceName);
        return audioService
            ?? throw new ValidationFailedException($"Invalid cognitive service name provided '{serviceName.ToString().ToLower()}'");
    }
}