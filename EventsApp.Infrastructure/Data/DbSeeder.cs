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
        await SeedDemoUsersAsync(ct);               // רק בסביבת פיתוח (Seed:DemoUsers = true)
    }

    public const string DemoPassword = "Demo1234!";     // סיסמת דמו גלויה בכוונה: משמשת רק לבדיקה מקומית ומתועדת ב-README

    private async Task SeedDemoUsersAsync(CancellationToken ct)
    {
        if (!_settings.DemoUsers) return;           // כבוי כברירת מחדל: בייצור (Render) לא נוצרים משתמשי דמו

        if (!await _db.Users.AnyAsync(u => u.Role == UserRole.Admin, ct))      // אין עדיין מנהל (לא הוגדר Seed:AdminEmail)
            AddDemoUser("admin@demo.local", "מנהל דמו", UserRole.Admin);       // מנהל דמו, כדי שהמערכת תהיה מלאה מיד

        if (!await _db.Users.AnyAsync(u => u.Email == "client@demo.local", ct))
            AddDemoUser("client@demo.local", "לקוחה דמו", UserRole.Client);    // לקוח דמו

        if (!await _db.Users.AnyAsync(u => u.Email == "provider@demo.local", ct))
        {
            var user = AddDemoUser("provider@demo.local", "ספק דמו", UserRole.Provider);   // ספק דמו
            var categoryId = await _db.Categories.OrderBy(c => c.Id).Select(c => c.Id).FirstAsync(ct);   // הקטגוריה הראשונה
            user.Provider = new Provider                // פרופיל הספק, כבר מאושר כדי שיוכל להגיש מועמדות מיד
            {
                BusinessName = "צילומי דמו",
                Description = "ספק לדוגמה לבדיקת המערכת",
                Phone = "050-0000000",
                City = "ירושלים",
                CategoryId = categoryId,
                StartingPrice = 3000,
                Status = ProviderStatus.Approved,
                AcceptedTermsAt = DateTime.UtcNow
            };
        }

        await _db.SaveChangesAsync(ct);             // שמירה אחת לכל משתמשי הדמו
        _logger.LogInformation("Demo users ensured");   // בלי סיסמאות בלוג
    }

    private User AddDemoUser(string email, string fullName, UserRole role)
    {
        var user = new User                         // משתמש חדש עם סיסמה מוצפנת (BCrypt), כמו בהרשמה רגילה
        {
            FullName = fullName,
            Email = email,
            PasswordHash = _hasher.Hash(DemoPassword),
            Role = role,
            CreatedAtUtc = DateTime.UtcNow
        };
        _db.Users.Add(user);                        // רק מסמן הוספה
        return user;                                // מוחזר כדי שאפשר יהיה להוסיף לו פרופיל ספק
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
        var admin = await _db.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin, ct);   // המנהל היחיד במערכת (אם כבר קיים)

        if (admin == null)                          // עדיין אין מנהל: יוצרים אותו מההגדרות
        {
            _db.Users.Add(new User
            {
                FullName = _settings.AdminFullName,
                Email = email,
                PasswordHash = _hasher.Hash(_settings.AdminPassword),
                Role = UserRole.Admin,
                CreatedAtUtc = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(ct);
            return;
        }

        if (admin.Email == email && _hasher.Verify(_settings.AdminPassword, admin.PasswordHash))
            return;                                 // המנהל כבר תואם להגדרות: אין מה לעדכן

        admin.Email = email;                        // ההגדרות השתנו: מעדכנים את אותו מנהל (ולא יוצרים מנהל שני)
        admin.FullName = _settings.AdminFullName;
        admin.PasswordHash = _hasher.Hash(_settings.AdminPassword);
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Admin credentials updated from configuration");    // בלי מייל וסיסמה בלוג
    }
}
