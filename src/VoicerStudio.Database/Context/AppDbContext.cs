using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NeerCore.Data.EntityFramework.Design;
using NeerCore.Data.EntityFramework.Extensions;
using VoicerStudio.Database.Entities;
using VoicerStudio.Database.Enums;

namespace VoicerStudio.Database.Context;

#pragma warning disable CS8618
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<AppUser> Users { get; set; }
    public DbSet<AppToken> Tokens { get; set; }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureEntities(options =>
        {
            options.EngineStrategy = DbEngineStrategy.SqlServer;
            options.ApplyEntityDating = true;
            options.ApplyEntityTypeConfigurations = true;
            options.DataAssemblies = new[] { typeof(AppDbContext).Assembly };
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<AccessStatus>().HaveConversion<EnumToStringConverter<AccessStatus>>();
        builder.Properties<UserRole>().HaveConversion<EnumToStringConverter<UserRole>>();
    }
}
#pragma warning restore CS8618