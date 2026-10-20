using EventsApp.Application.DTOs.Notifications;

namespace EventsApp.Application.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetMineAsync(int userId, CancellationToken ct);     // ההתראות שלי
    Task MarkReadAsync(int userId, int id, CancellationToken ct);                   // סימון כנקראה
}
