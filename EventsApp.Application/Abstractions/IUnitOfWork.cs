namespace EventsApp.Application.Abstractions;       // ליד שאר ההפשטות (IPasswordHasher, ITokenGenerator)

public interface IUnitOfWork                        // חוזה: שמירת כל השינויים בבת אחת
{
    Task SaveChangesAsync(CancellationToken ct);    // זורק ConcurrencyException או ConflictException במקרה התנגשות
    Task BeginTransactionAsync(CancellationToken ct);   // פותח טרנזקציה מפורשת: כל הקריאות והשמירות עד Commit שייכות אליה
    Task CommitAsync(CancellationToken ct);         // מאשר את הטרנזקציה: כל השינויים נשמרים סופית
    Task RollbackAsync(CancellationToken ct);       // מבטל את הטרנזקציה: שום שינוי ממנה לא נשמר
}
