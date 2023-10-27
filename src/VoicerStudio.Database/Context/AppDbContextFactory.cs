using Microsoft.EntityFrameworkCore;
using NeerCore.Data.EntityFramework.Design;

namespace VoicerStudio.Database.Context;

public class AppDbContextFactory : DbContextFactoryBase<AppDbContext>
{
    public override TextWriter? LogWriter => null;
    public override string SelectedConnectionName => "Default";

    public override string[] SettingsPaths => new[] { "appsettings.Development.json", "appsettings.json", "migration.json" };


    public override AppDbContext CreateDbContext(string[] args) => new(CreateContextOptions());

    public override void ConfigureContextOptions(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(ConnectionString,
            options => options.MigrationsAssembly(MigrationsAssembly));
    }
}