using System.Globalization;
using System.Text.Json.Serialization;
using Serilog;
using VoicerStudio.Api.Shared.Extensions;
using VoicerStudio.Api.Shared.Logging;
using VoicerStudio.Api.Shared.Middlewares;
using VoicerStudio.Application;
using VoicerStudio.Application.Models.Speech;

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

    builder.Services.AddApplication();

    builder.Services.AddCustomCors(builder.Configuration);
    builder.Services.AddCustomSwagger();
    builder.Services.AddCustomFluentValidation<SpeechGenerateRequest>();

    builder.Services.AddScoped<AppExceptionHandler>();
    builder.Services.AddControllers()
        .AddJsonOptions(json => json.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
}

static void ConfigureWebApp(WebApplication app)
{
    if (app.Environment.IsDevelopment())
        app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Voicer Studio API v1");
    });

    app.UseCors("Default");

    if (app.Environment.IsProduction())
        app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseMiddleware<AppExceptionHandler>();

    app.MapControllers();
}