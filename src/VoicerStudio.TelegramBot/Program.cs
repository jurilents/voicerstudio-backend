using System.Globalization;
using Serilog;
using VoicerStudio.Api.Shared.Logging;
using VoicerStudio.Api.Shared.Middlewares;
using VoicerStudio.Database;
using VoicerStudio.TelegramBot;

var logger = AppLoggerFactory.CreateLogger();
var culture = new CultureInfo("en-US");
CultureInfo.CurrentCulture = culture;
CultureInfo.DefaultThreadCurrentCulture = culture;

try
{
    var builder = WebApplication.CreateBuilder(args);
    logger.Debug("Configuring application builder...");
    ConfigureBuilder(builder);


    var app = builder.Build();
    logger.Debug("Running web application...");
    ConfigureWebApp(app);

    app.Services.MigrateDatabaseAsync().Wait();

    app.Run();
}
catch (Exception e)
{
    logger.Fatal(e, "Unhandled exception");
}
finally
{
    logger.Information("Application is now stopping...");
}


static void ConfigureBuilder(WebApplicationBuilder builder)
{
    builder.Host.UseSerilog();

    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddTelegramBot(builder.Configuration);

    builder.Services.AddScoped<AppExceptionHandler>();
    builder.Services.AddControllers().AddNewtonsoftJson();
}

static void ConfigureWebApp(WebApplication app)
{
    if (app.Environment.IsDevelopment())
        app.UseDeveloperExceptionPage();

    if (app.Environment.IsProduction())
        app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseMiddleware<AppExceptionHandler>();

    app.MapControllers();
}