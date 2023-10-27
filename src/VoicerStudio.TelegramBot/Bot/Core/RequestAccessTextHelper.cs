using VoicerStudio.Database.Entities;
using VoicerStudio.Database.Enums;

namespace VoicerStudio.TelegramBot.Bot.Core;

public static class RequestAccessTextHelper
{
    public static string GetText(AppUser user, DateTime messageDate, AppUser? adminUser, AccessStatus status)
    {
        var username = string.IsNullOrEmpty(user.TgUsername) ? "link" : $"@{user.TgUsername}";

        var statusText = adminUser is not null
            ? status switch
            {
                AccessStatus.Accepted => $"<b>✅ Accepted</b>  by @{adminUser.TgUsername} at {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC",
                AccessStatus.Rejected => $"<b>🚫 Rejected</b>  by @{adminUser.TgUsername} at {DateTime.UtcNow:dd.MM.yyyy HH:mm} UTC",
                _                     => "",
            }
            : "";

        return $"""
                ℹ️ New user wants to gain an access

                Name: <b>{user.FullName}</b>
                Username: <a href="tg://user?id={user.TgUserId}">{username}</a>
                Time: <b>{messageDate:dd.MM.yyyy HH:mm} UTC</b>

                {statusText}
                """;
    }
}