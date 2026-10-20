using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class EventItemRepository : IEventItemRepository
{
    private readonly AppDbContext _db;
    public EventItemRepository(AppDbContext db) { _db = db; }   // הזרקה

    public Task<EventItem?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.EventItems
            .Include(i => i.Event).Include(i => i.Category).Include(i => i.Provider)    // במעקב, עם כל מה שהשירות והמיפוי צריכים
            .FirstOrDefaultAsync(i => i.Id == id, ct);                                  // סעיף אחד או null

    public Task<List<EventItem>> GetOpenAsync(int categoryId, int providerId, DateOnly today, CancellationToken ct) =>
        _db.EventItems.AsNoTracking()                                   // קריאה בלבד
            .Include(i => i.Event).Include(i => i.Category)             // לתאריך, למקום ולשם הקטגוריה
            .Include(i => i.Candidacies.Where(c => c.ProviderId == providerId))     // טוען רק מועמדויות של הספק הזה, כדי לחשב AlreadyApplied
            .Where(i => i.CategoryId == categoryId                      // רק הקטגוריה של הספק
                        && i.ProviderId == null                         // רק סעיפים שעוד לא שובץ בהם ספק
                        && i.Event.Date > today)                        // רק אירועים עתידיים
            .OrderBy(i => i.Event.Date).ToListAsync(ct);                // הקרובים קודם

    public Task<int> CountByEventAsync(int eventId, CancellationToken ct) =>
        _db.EventItems.CountAsync(i => i.EventId == eventId, ct);               // ספירה בלי לטעון שורות

    public Task<List<EventItem>> GetBookedAsync(int providerId, CancellationToken ct) =>
        _db.EventItems.AsNoTracking()                                           // קריאה בלבד
            .Include(i => i.Event).ThenInclude(e => e.Client)                   // אירוע ולקוח (לשם ולאימייל)
            .Include(i => i.Category)                                           // שם קטגוריה
            .Where(i => i.ProviderId == providerId)                             // רק של הספק
            .OrderBy(i => i.Event.Date).ToListAsync(ct);                        // הקרובים קודם

    public Task<List<int>> GetAssignedProviderUserIdsAsync(int eventId, CancellationToken ct) =>
        _db.EventItems.Where(i => i.EventId == eventId && i.ProviderId != null) // סעיפים עם ספק באירוע
            .Select(i => i.Provider!.UserId)                                    // מזהה המשתמש של הספק
            .Distinct().ToListAsync(ct);                                        // בלי כפילויות (ספק אחד, כמה סעיפים: התראה אחת)

    public void Remove(EventItem item) => _db.EventItems.Remove(item);         // רק מסמן מחיקה (המועמדויות נמחקות ב-Cascade)
}
