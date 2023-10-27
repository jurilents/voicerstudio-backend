using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoicerStudio.Database.Entities;

namespace VoicerStudio.Database.Configurations;

public class AppTokenConfiguration : IEntityTypeConfiguration<AppToken>
{
    public void Configure(EntityTypeBuilder<AppToken> builder)
    {
        builder.ToTable($"{nameof(AppToken)}s").HasKey(x => x.UserToken);

        builder.Property(x => x.UserToken).HasMaxLength(64);
        builder.Property(x => x.JwtToken).HasMaxLength(512);

        builder.HasOne(x => x.User).WithMany(x => x.Tokens).HasForeignKey(x => x.UserId);
    }
}