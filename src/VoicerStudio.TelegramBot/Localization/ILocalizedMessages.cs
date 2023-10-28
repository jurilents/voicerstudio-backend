namespace VoicerStudio.TelegramBot.Localization;

public interface ILocalizedMessages
{
    string Language { get; }
    string StartWelcome();
    string SetLanguage();
    string NotifyAccessAccepted();
    string NotifyAccessRejected(string? adminUsername);
    string AccessRequestSent();
    string AccessRequestIsPending();
    string AccessRequestAlreadyAccepted();
    string AccessRequestAlreadyRejected(string? adminUsername);
    string CommonResponseForStatusNew();
    string CommonResponseForStatusAccepted();
    string CommonResponseForStatusRejected(string? adminUsername);
    string LoginTokenInfo(string userToken, DateTime expired);
}