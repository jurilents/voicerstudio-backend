using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VoicerStudio.Database.Entities;

namespace VoicerStudio.Database.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ToTable($"{nameof(AppUser)}s").HasKey(x => x.Id);
        builder.HasIndex(x => x.TgUserId).IsUnique();

        builder.Property(x => x.TgUsername).HasMaxLength(100);
        builder.Property(x => x.FullName).HasMaxLength(200);
        builder.Property(x => x.Language).HasMaxLength(2).IsUnicode(false);
        builder.Property(x => x.Status).HasMaxLength(16).IsUnicode(false);
        builder.Property(x => x.Role).HasMaxLength(16).IsUnicode(false);

        builder.HasOne(x => x.AdminWhoSetStatus).WithMany().HasForeignKey(x => x.AdminWhoSetStatusId);
    }
}