using FluentValidation;
using FluentValidation.AspNetCore;
using MicroElements.Swashbuckle.FluentValidation.AspNetCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using VoicerStudio.Application.Models.Speech;

namespace VoicerStudio.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddCustomCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = configuration.GetRequiredSection("Cors:Origins").Get<string[]>();
        if (origins is null) throw new Exception("You must setup 'Cors:Origins' array in appsettings.json");

        services.AddCors(options => options
            .AddPolicy("Default", cors => cors
                .WithOrigins(origins)
                .WithHeaders("Content-Type", "X-Credentials", "X-Duration")
                .AllowAnyMethod()
                .AllowCredentials()));
    }

    public static void AddCustomSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Voicer Studio API",
                Version = "v1",
                Description = "Azure & AWS text to speech wrapper",
            });

            c.MapType<TimeSpan>(() => new OpenApiSchema
            {
                Type = "string",
                Example = new OpenApiString("00:00:00.0000000")
            });

            var xmlFiles = new[] { "VoicerStudio.Api.xml", "VoicerStudio.Application.xml" };
            foreach (var xmlFile in xmlFiles)
            {
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            }
        });
    }

    public static void AddCustomFluentValidation(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation(fv =>
            fv.DisableDataAnnotationsValidation = true);
        services.AddFluentValidationClientsideAdapters();
        services.AddValidatorsFromAssemblyContaining<SpeechGenerateRequest>(ServiceLifetime.Transient);
        services.AddFluentValidationRulesToSwagger();
    }
}