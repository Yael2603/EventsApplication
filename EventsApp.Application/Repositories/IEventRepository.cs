using EventsApp.Domain.Entities;

namespace EventsApp.Application.Repositories;

public interface IEventRepository                   // גישה לאירועים
{
    void Add(Event ev);                             // מוסיף למעקב; בכוונה בלי שמירה (השמירה דרך IUnitOfWork)
    Task<Event?> GetByIdAsync(int id, CancellationToken ct);            // אירוע עם לקוח, סעיפים, קטגוריות וספקים
    Task<List<Event>> GetByClientAsync(int clientId, CancellationToken ct);   // האירועים של לקוח
    Task<List<Event>> GetAllAsync(CancellationToken ct);                // כל האירועים (לאדמין)
    void Remove(Event ev);                          // חדש: מסמן מחיקת אירוע (בלי שמירה)
    Task<Event?> GetForUpdateAsync(int id, CancellationToken ct);       // חדש: אירוע במעקב עם סעיפיו (לעריכה)
}
