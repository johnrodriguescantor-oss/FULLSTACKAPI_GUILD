using FullStackAPI_Guild.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace FullStackAPI_Guild.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ClaCode> ClaCodes => Set<ClaCode>();
    public DbSet<RoleChangeHistory> RoleChangeHistories => Set<RoleChangeHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Username).IsUnique();
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.Username).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<ClaCode>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code);
            entity.Property(x => x.Code).HasMaxLength(6).IsRequired();

            entity.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.UsedByUser)
                .WithMany()
                .HasForeignKey(x => x.UsedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RoleChangeHistory>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.TargetUser)
                .WithMany()
                .HasForeignKey(x => x.TargetUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ChangedByUser)
                .WithMany()
                .HasForeignKey(x => x.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(x => x.Reason).HasMaxLength(250);
        });
    }
}