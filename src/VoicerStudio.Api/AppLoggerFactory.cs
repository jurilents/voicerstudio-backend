using Serilog;
using Serilog.Events;
using ILogger = Serilog.ILogger;

namespace VoicerStudio.Api;

public static class AppLoggerFactory
{
    public static ILogger CreateLogger()
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("VoicerStudio", LogEventLevel.Debug)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine("logs/.log"),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 20 * 1024 * 1024,
                retainedFileCountLimit: 7,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
            .CreateLogger();
    }
}