using cursor_dotnet_test.Persistence.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace cursor_dotnet_test.Persistence;

public class SoccerDbContext : IdentityDbContext<IdentityUser>
{
    public DbSet<TeamDataModel> Teams { get; set; }
    public DbSet<PlayerDataModel> Players { get; set; }

    public SoccerDbContext(DbContextOptions<SoccerDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<TeamDataModel>(entity =>
        {
            entity.ToTable("Teams");
            entity.HasIndex(t => t.ManagerName).IsUnique();
        });

        builder.Entity<PlayerDataModel>(entity =>
        {
            entity.ToTable("Players", t =>
            {
                t.HasCheckConstraint("CK_Players_TeamId_NotEmpty",
                    "\"TeamId\" != '00000000-0000-0000-0000-000000000000'");
                t.HasCheckConstraint("CK_Players_PlayerAge_Positive",
                    "\"PlayerAge\" > 0");
                t.HasCheckConstraint("CK_Players_PlayerPosition_NotEmpty",
                    "length(\"PlayerPosition\") > 0");
            });
            entity.HasIndex(p => p.TeamId);
        });
    }
}
