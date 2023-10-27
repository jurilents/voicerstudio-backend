namespace VoicerStudio.TelegramBot.Bot.Models;

public record JwtToken(
    string Token,
    DateTime Expires
);