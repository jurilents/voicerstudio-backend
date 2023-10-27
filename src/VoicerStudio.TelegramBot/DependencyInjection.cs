using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NeerCore.DependencyInjection.Extensions;
using Telegram.Bot;
using VoicerStudio.TelegramBot.Bot.Commands;
using VoicerStudio.TelegramBot.Bot.Core;
using VoicerStudio.TelegramBot.Bot.Localization;
using VoicerStudio.TelegramBot.HostedServices;

namespace VoicerStudio.TelegramBot;

public static class DependencyInjection
{
    public static void AddTelegramBot(this IServiceCollection services)
    {
        services.AddAllServices(injection => injection.ResolveInternalImplementations = true);

        // Command handlers
        services.AddTransient<CommandHandler, LoginCommandHandler>();
        services.AddTransient<CommandHandler, RequestAccessCommandHandler>();
        services.AddTransient<CommandHandler, SetLanguageCommandHandler>();
        services.AddTransient<CommandHandler, StartCommandHandler>();

        // Callback query handlers
        services.AddTransient<CallbackQueryHandler, RequestAccessCallbackHandler>();

        // Localization services
        services.AddSingleton<LocalizedMessagesManager>();
        services.AddSingleton<ILocalizedMessages, EnglishLocalizedMessages>();
        services.AddSingleton<ILocalizedMessages, RussianLocalizedMessages>();


        services.AddHostedService<ConfigureWebhookHostedService>();

        // Register named HttpClient to get benefits of IHttpClientFactory
        // and consume it with ITelegramBotClient typed client.
        // More read:
        //  https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests#typed-clients
        //  https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests
        services.AddHttpClient(nameof(TelegramBotClient))
            .AddTypedClient<ITelegramBotClient>((httpClient, sp) =>
            {
                var config = sp.GetRequiredService<IOptions<TelegramOptions>>().Value;
                var options = new TelegramBotClientOptions(config.BotToken);
                return new TelegramBotClient(options, httpClient);
            });
    }
}