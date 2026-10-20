using EventsApp.Application.Abstractions;
using EventsApp.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace EventsApp.Infrastructure.Data;            // לצד AppDbContext, כי זה מימוש שתלוי ב-EF

public class UnitOfWork : IUnitOfWork               // מממש את החוזה מ-Application
{
    private readonly AppDbContext _db;              // אותו DbContext שמשותף למאגרים באותה בקשה
    private readonly ILogger<UnitOfWork> _logger;  // לוג להתנגשויות
    private IDbContextTransaction? _transaction;    // הטרנזקציה הפתוחה כרגע (null אם אין)

    public UnitOfWork(AppDbContext db, ILogger<UnitOfWork>? logger = null)     // הזרקה; ה-logger אופציונלי כדי שהבדיקות יוכלו ליצור בלי לוג
    {
        _db = db;
        _logger = logger ?? NullLogger<UnitOfWork>.Instance;   // בלי logger: לוג "ריק" שלא כותב כלום
    }

    public async Task BeginTransactionAsync(CancellationToken ct)
    {
        if (_transaction != null)                   // הגנה: לא פותחים טרנזקציה בתוך טרנזקציה
            throw new InvalidOperationException("A transaction is already open");
        _transaction = await _db.Database.BeginTransactionAsync(ct);    // BEGIN בבסיס הנתונים; מעכשיו כל SELECT ו-SaveChanges על אותו חיבור בתוכה
        _logger.LogDebug("Transaction started");    // Debug: מידע לפיתוח בלבד (לא נכתב בייצור)
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        if (_transaction == null) return;           // אין טרנזקציה פתוחה: אין מה לאשר
        await _transaction.CommitAsync(ct);         // COMMIT: השינויים נשמרים סופית
        _logger.LogDebug("Transaction committed");  // Debug: מידע לפיתוח בלבד
        await _transaction.DisposeAsync();          // משחרר את האובייקט
        _transaction = null;                        // מאפשר טרנזקציה חדשה באותה בקשה
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_transaction == null) return;           // אין טרנזקציה פתוחה: אין מה לבטל
        await _transaction.RollbackAsync(ct);       // ROLLBACK: מבטל כל מה שנעשה בטרנזקציה
        _logger.LogDebug("Transaction rolled back");    // Debug: מידע לפיתוח בלבד
        await _transaction.DisposeAsync();          // משחרר את האובייקט
        _transaction = null;                        // מאפשר טרנזקציה חדשה באותה בקשה
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);         // שמירה אחת של כל השינויים (טרנזקציה אחת)
        }
        catch (DbUpdateConcurrencyException ex)     // מישהו שינה את אותה שורה לפנינו (Version לא תואם)
        {
            _logger.LogWarning("Concurrency conflict on {Entities}",  // התנגשות על משאב: Warning (חריג שטופל, יחזור 409)
                string.Join(", ", ex.Entries.Select(e => e.Metadata.ClrType.Name)));   // רק שמות הישויות, בלי נתונים אישיים
            throw new ConcurrencyException("הנתונים השתנו בינתיים על ידי משתמש אחר. יש לרענן ולנסות שוב");   // יהפוך ל-409
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })   // הפרת אינדקס ייחודי ב-PostgreSQL
        {
            _logger.LogWarning("Unique constraint conflict: {Constraint}",  // התנגשות על אינדקס ייחודי (למשל אירוע שני באותו תאריך): Warning
                (ex.InnerException as PostgresException)?.ConstraintName);  // רק שם האילוץ
            throw new ConflictException("הרשומה כבר קיימת במערכת");   // יהפוך ל-409
        }
    }
}
