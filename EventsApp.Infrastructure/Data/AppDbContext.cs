using EventsApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<CompanyDay> CompanyDays => Set<CompanyDay>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventItem> EventItems => Set<EventItem>();
    public DbSet<Candidacy> Candidacies => Set<Candidacy>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.Property(x => x.FullName).HasMaxLength(100);
            e.Property(x => x.Email).HasMaxLength(200);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Category>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100);
            e.HasIndex(x => x.Name).IsUnique();
        });

        b.Entity<Provider>(e =>
        {
            e.Property(x => x.BusinessName).HasMaxLength(150);
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.City).HasMaxLength(100);
            e.Property(x => x.StartingPrice).HasPrecision(12, 2);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);

            e.HasOne(x => x.User).WithOne(u => u.Provider)
                .HasForeignKey<Provider>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => x.UserId).IsUnique();

            e.HasOne(x => x.Category).WithMany(c => c.Providers)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<CompanyDay>(e =>
        {
            e.HasKey(x => x.Date);
        });

        b.Entity<Event>(e =>
        {
            e.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Location).HasMaxLength(200);
            e.Property(x => x.Notes).HasMaxLength(1000);

            e.HasOne(x => x.Client).WithMany(u => u.Events)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Day).WithMany()
                .HasForeignKey(x => x.Date)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasIndex(x => x.Date).IsUnique();
        });

        b.Entity<EventItem>(e =>
        {
            e.Property(x => x.Budget).HasPrecision(12, 2);
            e.Property(x => x.Details).HasMaxLength(1000);

            e.HasOne(x => x.Event).WithMany(ev => ev.Items)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Category).WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Provider).WithMany(p => p.BookedItems)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<Candidacy>(e =>
        {
            e.Property(x => x.Message).HasMaxLength(1000);
            e.Property(x => x.ProposedPrice).HasPrecision(12, 2);

            e.HasOne(x => x.EventItem).WithMany(i => i.Candidacies)
                .HasForeignKey(x => x.EventItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Provider).WithMany(p => p.Candidacies)
                .HasForeignKey(x => x.ProviderId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.EventItemId, x.ProviderId }).IsUnique();
        });
    }

    // PostgreSQL אין rowversion, ולכן Version של CompanyDay מתחלף ידנית בכל עדכון
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        RefreshDayVersions();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        RefreshDayVersions();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void RefreshDayVersions()
    {
        foreach (var entry in ChangeTracker.Entries<CompanyDay>()
                     .Where(e => e.State == EntityState.Modified))
        {
            entry.Entity.Version = Guid.NewGuid();
        }
    }
}
