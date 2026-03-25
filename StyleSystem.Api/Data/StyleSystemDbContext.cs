using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StyleSystem.Api.Entities;

namespace StyleSystem.Api.Data;

public class StyleSystemDbContext(
    DbContextOptions<StyleSystemDbContext> options) : DbContext(options)
{
    public required DbSet<User> Users { get; set; }
    public required DbSet<Recommendation> Recommendations { get; set; }
    public required DbSet<RecommendationImage> RecommendationImages { get; set; }
    
    public async new ValueTask<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateCreatedAt(ChangeTracker.Entries<IHasCreatedAt>());
        return await base.SaveChangesAsync(cancellationToken);
    }

    private static void UpdateCreatedAt(IEnumerable<EntityEntry<IHasCreatedAt>> entries)
    {
        foreach(var entry in entries)
            if(entry.State is EntityState.Added)
                entry.Entity.CreatedAt = DateTimeOffset.UtcNow;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.FullName)
                .HasMaxLength(200)
                .IsRequired(true);

            entity.Property(u => u.Username)
                .HasMaxLength(100)
                .IsRequired(true);

            entity.HasIndex(u => u.Username)
                .IsUnique();

            entity.Property(u => u.PasswordHash)
                .IsRequired(true)
                .HasMaxLength(256);
            
            entity.Property(u => u.Age)
                .IsRequired(false);

            entity.Property(e => e.Height)
                .IsRequired(false);

            entity.Property(e => e.Weight)
                .IsRequired(false);

            entity.Property(e => e.Gender)
                .IsRequired(false)
                .HasConversion<string>();

            entity.Property(e => e.MaleBodyType)
                .IsRequired(false)
                .HasConversion<string>();

            entity.Property(e => e.FemaleBodyType)
                .IsRequired(false)
                .HasConversion<string>();

            entity.Property(e => e.SkinTone)
                .IsRequired(false)
                .HasConversion<string>();
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");
                        
            entity.HasMany(e => e.Recommendations)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<RecommendationImage>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.Order)
                .IsRequired(true);

            entity.Property(e => e.RecommendationId)
                .IsRequired();

            entity.HasIndex(e => e.RecommendationId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.Recommendation)
                .WithMany(e => e.Images)
                .HasForeignKey(e => e.RecommendationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}