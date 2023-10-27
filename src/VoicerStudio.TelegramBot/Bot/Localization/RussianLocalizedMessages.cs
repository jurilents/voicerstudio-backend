namespace VoicerStudio.TelegramBot.Bot.Localization;

public class RussianLocalizedMessages : ILocalizedMessages
{
    public string Language => "ru";

    private const string DefaultAdminUsername = "jurilents";

    public string StartWelcome() =>
        """
        Привет! 👋
        Перед началом работы с ботом, вам необходимо получить доступ.

        Для этого нажми на кнопку ниже или введи команду /request_access

        Для смены языка общения с ботом можете использовать одну из следующих команд: /lang_rus или /lang_eng
        """;

    public string SetLanguage() => "Ваш язык изменет на русский  🎻";

    public string NotifyAccessAccepted() =>
        """
        Хей, как дела?
        У нас есть хорошие новости для вас! Ваш запрос был <b>принят</b> 🎉

        <i>Используйте команду /login чтобы получить свой первый ключ доступа!</i>
        """;

    public string NotifyAccessRejected(string? adminUsername) =>
        $"""
         Хей, как дела?
         К сожалению, ваш запрос был <b>отклонен</b>. 😔

         <i>Если вы считаете, что это ошибка, пожалуйста, свяжитесь с @{adminUsername ?? DefaultAdminUsername}</i>
         """;

    public string AccessRequestSent() => "Ура! Мы уведомим вас, когда ваш запрос доступа будет обработан 👌";
    public string AccessRequestIsPending() => "Вас запрос ещё в обработке. Это может занять какое-то время 🕐";

    public string AccessRequestAlreadyAccepted() =>
        """
        У вас уже есть доступ к боту 👌

        <i>Используйте команду /login для получения ключа доступа</i>
        """;

    public string AccessRequestAlreadyRejected(string? adminUsername) =>
        $"""
         Ой, ваш запрос доступа был отклонен. 😔

         <i>Если вы считаете, что это ошибка, пожалуйста, свяжитесь с @{adminUsername ?? DefaultAdminUsername}</i>
         """;

    public string CommonResponseForStatusNew() =>
        """
        Йо! 🙌
        Ваш запрос ещё в обработке. Мы уведомим вас, когда ваш запрос доступа будет обработан 👌
        """;

    public string CommonResponseForStatusAccepted() =>
        """
        Йо! 👋
        У вас уже есть доступ к боту, просто используйте команду /login для получения вашего персонального ключа
        """;

    public string CommonResponseForStatusRejected(string? adminUsername) =>
        $"""
         Йо! 🙌
         Простите, но ваш запрос доступа был отклонен. 😔

         <i>Если вы считаете, что это ошибка, пожалуйста, свяжитесь с @{adminUsername ?? DefaultAdminUsername}</i>
         """;

    public string LoginTokenInfo(string userToken, DateTime expired) =>
        $"""
         Ваш ключ для доступа:
         <code>{userToken}</code>
         <i>(Нажмите, чтобы скопировать)</i>

         Активен до <b>{expired:dd.MM.yyyy HH:mm} UTC</b>
         """;
}