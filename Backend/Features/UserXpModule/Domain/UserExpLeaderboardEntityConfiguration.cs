using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PureTCOWebApp.Features.UserXpModule.Domain;

public class UserExpLeaderboardEntityConfiguration : IEntityTypeConfiguration<UserExpLeaderboard>
{
    public void Configure(EntityTypeBuilder<UserExpLeaderboard> builder)
    {
        builder.ToView("user_exp_leaderboard");
        
        builder.HasNoKey();

        builder.Property(x => x.UserId)
            .HasColumnName("user_id");

        builder.Property(x => x.TotalExp)
            .HasColumnName("total_exp")
            .HasColumnType("decimal(18,2)");
    }
}
