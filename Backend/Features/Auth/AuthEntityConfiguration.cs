using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.Auth;

public class AuthEntityConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Nim)
            .HasMaxLength(50);

        builder.Property(u => u.ProgramStudy)
            .HasMaxLength(100);
    }
}