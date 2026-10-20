using EventsApp.Domain.Entities;

namespace EventsApp.Application.Repositories;

public interface IEventItemRepository               // גישה לסעיפי אירועים
{
    Task<EventItem?> GetByIdAsync(int id, CancellationToken ct);        // סעיף במעקב עם אירוע, קטגוריה וספק
    Task<List<EventItem>> GetOpenAsync(int categoryId, int providerId, DateOnly today, CancellationToken ct);   // סעיפים פתוחים לספק
    Task<int> CountByEventAsync(int eventId, CancellationToken ct);                 // חדש: כמה סעיפים באירוע
    Task<List<EventItem>> GetBookedAsync(int providerId, CancellationToken ct);     // חדש: הסעיפים ששובצו לספק
    Task<List<int>> GetAssignedProviderUserIdsAsync(int eventId, CancellationToken ct);     // חדש: משתמשי הספקים ששובצו באירוע
    void Remove(EventItem item);                                                    // חדש: מסמן מחיקת סעיף
}
