using NeerCore.DependencyInjection;

namespace VoicerStudio.TelegramBot.Bot.Localization;

[Service]
public class LocalizedMessagesManager
{
    private readonly IList<ILocalizedMessages> _localizedMessages;

    public LocalizedMessagesManager(IEnumerable<ILocalizedMessages> localizedMessages)
    {
        _localizedMessages = localizedMessages.ToList();
    }

    public ILocalizedMessages FromLanguage(string languageCode)
    {
        var service = _localizedMessages.FirstOrDefault(x =>
            string.Equals(x.Language, languageCode, StringComparison.InvariantCultureIgnoreCase));
        return service ?? _localizedMessages.First(x => x.Language == "en");
    }
}