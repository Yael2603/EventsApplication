using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.Repositories;

public interface IDeletionRequestRepository         // גישה לבקשות מחיקה
{
    void Add(EventDeletionRequest request);                                         // מסמן הוספה
    Task<EventDeletionRequest?> GetByIdAsync(int id, CancellationToken ct);         // במעקב, עם אירוע ולקוח
    Task<bool> HasPendingAsync(int eventId, CancellationToken ct);                  // האם כבר יש בקשה ממתינה לאירוע
    Task<List<EventDeletionRequest>> GetListAsync(DeletionRequestStatus? status, CancellationToken ct);     // רשימה לקריאה בלבד
}
