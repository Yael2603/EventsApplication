using EventsApp.Application.DTOs.Notifications;

namespace EventsApp.Application.Services;

public interface INotificationService
{
    Task<List<NotificationDto>> GetMineAsync(int userId, CancellationToken ct);     // ההתראות שלי
    Task SetReadAsync(int userId, int id, bool isRead, CancellationToken ct);       // עדכון מצב קריאה (נקראה / לא נקראה)
}
