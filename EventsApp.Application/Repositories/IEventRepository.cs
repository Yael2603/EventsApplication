using EventsApp.Domain.Entities;

namespace EventsApp.Application.Repositories;

public interface IEventRepository                   // גישה לאירועים
{
    void Add(Event ev);                             // מוסיף למעקב; בכוונה בלי שמירה (השמירה דרך IUnitOfWork)
    Task<Event?> GetByIdAsync(int id, CancellationToken ct);            // אירוע עם לקוח, סעיפים, קטגוריות וספקים
    Task<List<Event>> GetByClientAsync(int clientId, CancellationToken ct);   // האירועים של לקוח
    Task<List<Event>> GetPageAsync(int page, int pageSize, CancellationToken ct);  // עמוד אחד של אירועים (לאדמין), Skip/Take בבסיס הנתונים
    Task<int> CountAsync(CancellationToken ct);                         // כמה אירועים יש בסך הכול (לחישוב מספר העמודים)
    void Remove(Event ev);                          // חדש: מסמן מחיקת אירוע (בלי שמירה)
    Task<Event?> GetForUpdateAsync(int id, CancellationToken ct);       // חדש: אירוע במעקב עם סעיפיו (לעריכה)
}
