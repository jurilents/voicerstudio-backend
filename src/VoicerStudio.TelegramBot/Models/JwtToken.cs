namespace VoicerStudio.TelegramBot.Models;

public record JwtToken(
    string Token,
    DateTime Expires
);