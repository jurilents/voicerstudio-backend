using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NeerCore.DependencyInjection.Extensions;
using VoicerStudio.Database.Context;
using VoicerStudio.Database.Options;

namespace VoicerStudio.Database;

public static class DependencyInjection
{
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAllServices(injection => injection.ResolveInternalImplementations = true);

        services.Configure<SecurityOptions>(configuration.GetRequiredSection("Security").Bind);

        var contextFactory = new AppDbContextFactory();
        services.AddDbContext<AppDbContext>(cob => contextFactory.ConfigureContextOptions(cob));
    }


    public static async Task MigrateDatabaseAsync(this IServiceProvider servicesProvider)
    {
        using var scope = servicesProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }
}