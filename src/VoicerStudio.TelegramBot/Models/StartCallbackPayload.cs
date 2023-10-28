using VoicerStudio.Database.Entities;
using VoicerStudio.Database.Enums;

namespace VoicerStudio.TelegramBot.Models;

public record StartCallbackPayload(
    AccessStatus Status,
    long UserId
)
{
    public static StartCallbackPayload FromUser(AppUser user) => new(user.Status, user.TgUserId);
}