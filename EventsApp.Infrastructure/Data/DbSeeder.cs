using EventsApp.Application.Abstractions;
using EventsApp.Application.Settings;
using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EventsApp.Infrastructure.Data;

public class DbSeeder
{
    private static readonly string[] CategoryNames =
    {
        "צילום סטילס", "צילום וידאו", "קייטרינג", "מוזיקה והגברה",
        "עיצוב ופרחים", "תאורה והפקה", "מלצרות", "אטרקציות לילדים"
    };

    private readonly AppDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly SeedSettings _settings;
    private readonly ILogger<DbSeeder> _logger;

    public DbSeeder(AppDbContext db, IPasswordHasher hasher,
        IOptions<SeedSettings> settings, ILogger<DbSeeder> logger)
    {
        _db = db;
        _hasher = hasher;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        await SeedCategoriesAsync(ct);
        await SeedCompanyDaysAsync(ct);
        await SeedAdminAsync(ct);
    }

    private async Task SeedCategoriesAsync(CancellationToken ct)
    {
        if (await _db.Categories.AnyAsync(ct)) return;

        _db.Categories.AddRange(CategoryNames.Select(n => new Category { Name = n }));
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedCompanyDaysAsync(CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var last = await _db.CompanyDays.MaxAsync(d => (DateOnly?)d.Date, ct);
        var start = last is null ? today.AddDays(1) : last.Value.AddDays(1);
        var end = today.AddDays(365);
        if (start > end) return;

        var days = new List<CompanyDay>();
        for (var d = start; d <= end; d = d.AddDays(1))
            days.Add(new CompanyDay { Date = d, IsTaken = false, Version = Guid.NewGuid() });

        _db.CompanyDays.AddRange(days);
        await _db.SaveChangesAsync(ct);
    }

    private async Task SeedAdminAsync(CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(_settings.AdminEmail)
            || string.IsNullOrWhiteSpace(_settings.AdminPassword))
        {
            _logger.LogWarning("Seed:AdminEmail / Seed:AdminPassword are not configured. Skipping admin creation.");
            return;
        }

        var email = _settings.AdminEmail.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(u => u.Email == email, ct)) return;

        _db.Users.Add(new User
        {
            FullName = _settings.AdminFullName,
            Email = email,
            PasswordHash = _hasher.Hash(_settings.AdminPassword),
            Role = UserRole.Admin,
            CreatedAtUtc = DateTime.UtcNow
        });
        await _db.SaveChangesAsync(ct);
    }
}
