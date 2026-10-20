using EventsApp.Domain.Entities;

namespace EventsApp.Application.Repositories;

public interface INotificationRepository            // גישה להתראות
{
    void Add(Notification notification);                                        // מסמן הוספה
    Task<List<Notification>> GetByUserAsync(int userId, CancellationToken ct);  // ההתראות של משתמש
    Task<Notification?> GetByIdAsync(int id, CancellationToken ct);             // התראה במעקב (לסימון כנקראה)
}
