using AutoMapper;
using EventsApp.Application.Abstractions;
using EventsApp.Application.DTOs.Notifications;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;

namespace EventsApp.Application.Services;

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notifications;    // התראות
    private readonly IUnitOfWork _uow;                          // שמירה
    private readonly IMapper _mapper;                           // המרה

    public NotificationService(INotificationRepository notifications, IUnitOfWork uow, IMapper mapper)  // הזרקה
    {
        _notifications = notifications;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<List<NotificationDto>> GetMineAsync(int userId, CancellationToken ct)
    {
        var list = await _notifications.GetByUserAsync(userId, ct);     // של המשתמש בלבד
        return _mapper.Map<List<NotificationDto>>(list);                // ממיר
    }

    public async Task SetReadAsync(int userId, int id, bool isRead, CancellationToken ct)
    {
        var notification = await _notifications.GetByIdAsync(id, ct)    // ההתראה (במעקב)
                           ?? throw new NotFoundException("ההתראה לא נמצאה");   // 404
        if (notification.UserId != userId)                              // של משתמש אחר
            throw new ForbiddenException("ניתן לסמן רק התראה שלך");     // 403

        notification.IsRead = isRead;                                   // המצב החדש מגוף הבקשה
        await _uow.SaveChangesAsync(ct);                                // שומר
    }
}
