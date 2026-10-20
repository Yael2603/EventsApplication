using EventsApp.Application.Abstractions;
using EventsApp.Application.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace EventsApp.Infrastructure.Data;            // לצד AppDbContext, כי זה מימוש שתלוי ב-EF

public class UnitOfWork : IUnitOfWork               // מממש את החוזה מ-Application
{
    private readonly AppDbContext _db;              // אותו DbContext שמשותף למאגרים באותה בקשה
    public UnitOfWork(AppDbContext db) { _db = db; }    // הזרקה

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try
        {
            await _db.SaveChangesAsync(ct);         // שמירה אחת של כל השינויים (טרנזקציה אחת)
        }
        catch (DbUpdateConcurrencyException)        // מישהו שינה את אותה שורה לפנינו (Version לא תואם)
        {
            throw new ConcurrencyException("הנתונים השתנו בינתיים על ידי משתמש אחר. יש לרענן ולנסות שוב");   // יהפוך ל-409
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })   // הפרת אינדקס ייחודי ב-PostgreSQL
        {
            throw new ConflictException("הרשומה כבר קיימת במערכת");   // יהפוך ל-409
        }
    }
}
