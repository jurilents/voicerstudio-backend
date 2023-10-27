namespace VoicerStudio.Application.Options;

public class TelegramOptions
{
    public string BotToken { get; set; } = null!;
    public string BotUsername { get; set; } = null!;
    public long AdminChatId { get; set; }
    public string WebhookUrl { get; set; } = null!;
    public string SecretToken { get; set; } = null!;
}