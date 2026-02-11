using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using StyleSystem.Api.Entities;

namespace StyleSystem.Api.Data;

public class StyleSystemDbContext(
    DbContextOptions<StyleSystemDbContext> options) : DbContext(options)
{
    public required DbSet<User> Users { get; set; }
    public required DbSet<FashionRecommendation> Recommendations { get; set; }
    public required DbSet<Chat> Chats { get; set; }
    public required DbSet<ChatMessage> Messages { get; set; }
    
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
            
            entity.HasMany(e => e.Chats)
                .WithOne(ch => ch.User)
                .HasForeignKey(ch => ch.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<FashionRecommendation>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TextRecommendation)
                .HasColumnType("text")
                .IsRequired(false);

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            entity.Property(e => e.ImagePrompt)
                .HasColumnType("text")
                .IsRequired(false);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            entity.Property(e => e.UserId)
                .IsRequired();

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CreatedAt);

            entity.HasOne(e => e.User)
                .WithMany(e => e.Recommendations)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Chat>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Title)
                .HasMaxLength(200)
                .IsRequired(false);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Chats)
                .HasForeignKey(ch => ch.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasMany(ch => ch.Messages)
                .WithOne(m => m.Chat)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ChatMessage>(e =>
        {
            e.HasKey(e => e.Id);

            e.Property(e => e.Content)
                .HasColumnType("text")
                .IsRequired(false);
            
            e.Property(e => e.Role)
                .IsRequired(true)
                .HasConversion<string>();
            
            e.HasOne(e => e.Chat)
                .WithMany(ch => ch.Messages)
                .HasForeignKey(m => m.ChatId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}