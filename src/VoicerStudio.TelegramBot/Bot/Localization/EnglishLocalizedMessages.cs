namespace VoicerStudio.TelegramBot.Bot.Localization;

public class EnglishLocalizedMessages : ILocalizedMessages
{
    public string Language => "en";

    private const string DefaultAdminUsername = "jurilents";

    public string StartWelcome() =>
        """
        Hey! 👋
        To gain an access to the bot, you have to send a request to the administrator.

        Press the button below to send a request or type /request_access.

        To set the language, type /lang_rus or /lang_eng.
        """;

    public string SetLanguage() => "Your language has been changed to English ☕️";

    public string NotifyAccessAccepted() =>
        """
        Hey, are you here?
        We have good news for you! Your request has been <b>accepted</b>. 🎉

        <i>Type /login to get your first access key!</i>
        """;

    public string NotifyAccessRejected(string? adminUsername) =>
        $"""
         Hey, are you here?
         Your request has been <b>rejected</b>. 😔

         <i>If you think it's a mistake, please contact the admin @{adminUsername ?? DefaultAdminUsername}.</i>
         """;

    public string AccessRequestSent() => "Great! You will be notified when your request processed. 👌";
    public string AccessRequestIsPending() => "Your request is still being processed. Please wait for a while. 🕐";

    public string AccessRequestAlreadyAccepted() =>
        """
        Hey, you already have an access to the bot. 👌

        <i>Type /login to get your an access key.</i>
        """;

    public string AccessRequestAlreadyRejected(string? adminUsername) =>
        $"""
         Sorry, your request has been rejected. 😔

         <i>If you think it's a mistake, please contact the admin @{adminUsername ?? DefaultAdminUsername}.</i>
         """;

    public string CommonResponseForStatusNew() =>
        """
        Hey! 🙌
        Your request is still pending. You will be notified when your request processed. 👌
        """;

    public string CommonResponseForStatusAccepted() =>
        """
        Hey! 👋
        Whats up? You already have an access to the bot. Type /login to get an access key.
        """;

    public string CommonResponseForStatusRejected(string? adminUsername) =>
        $"""
         Hey! 🙌
         Sorry, but your request was rejected. You can try to contact an admin directly @{adminUsername ?? DefaultAdminUsername}
         """;

    public string LoginTokenInfo(string userToken, DateTime expired) =>
        $"""
         There is your access key:
         <code>{userToken}</code>
         <i>(Click to copy)</i>

         Expired on <b>{expired:dd.MM.yyyy HH:mm} UTC</b>
         """;
}