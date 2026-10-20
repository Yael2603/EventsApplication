using AutoMapper;
using EventsApp.Application.Abstractions;
using EventsApp.Application.DTOs.DeletionRequests;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.Services;

public class DeletionRequestService : IDeletionRequestService
{
    private readonly IDeletionRequestRepository _requests;  // בקשות
    private readonly IEventRepository _events;              // אירועים
    private readonly ICalendarRepository _calendar;         // לוח (לשחרור היום)
    private readonly IEventItemRepository _items;           // חדש: סעיפים (למציאת ספקים ששובצו)
    private readonly INotificationRepository _notifications;    // חדש: התראות
    private readonly IUnitOfWork _uow;                      // שמירה אחת
    private readonly IMapper _mapper;                       // המרה

    public DeletionRequestService(IDeletionRequestRepository requests, IEventRepository events,
        ICalendarRepository calendar, IEventItemRepository items, INotificationRepository notifications,
        IUnitOfWork uow, IMapper mapper)            // הזרקה (שתי תלויות חדשות)
    {
        _requests = requests;
        _events = events;
        _calendar = calendar;
        _items = items;
        _notifications = notifications;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<DeletionRequestDto> CreateAsync(int clientId, int eventId, CreateDeletionRequestDto dto, CancellationToken ct)
    {
        var ev = await _events.GetByIdAsync(eventId, ct)                // האירוע (קריאה בלבד מספיקה כאן)
                 ?? throw new NotFoundException("האירוע לא נמצא");      // 404

        if (ev.ClientId != clientId)                                    // לא הבעלים
            throw new ForbiddenException("ניתן לבקש מחיקה רק לאירוע שלך");  // 403
        if (ev.Date <= DateOnly.FromDateTime(DateTime.UtcNow))          // אירוע שעבר
            throw new BadRequestException("לא ניתן לבקש מחיקה לאירוע שהתקיים");    // 400
        if (await _requests.HasPendingAsync(eventId, ct))               // כבר יש בקשה ממתינה
            throw new ConflictException("כבר קיימת בקשת מחיקה ממתינה לאירוע זה");  // 409

        var request = new EventDeletionRequest                          // בקשה חדשה
        {
            EventId = eventId,                                          // האירוע
            Reason = dto.Reason.Trim(),                                 // הנימוק (בלי רווחים בקצוות)
            CreatedAtUtc = DateTime.UtcNow                              // זמן הגשה (Status ברירת מחדל: Pending)
        };
        _requests.Add(request);                                         // מסמן הוספה
        await _uow.SaveChangesAsync(ct);                                // שומר; האינדקס החלקי הוא רשת ביטחון ← 409

        var saved = await _requests.GetByIdAsync(request.Id, ct);       // טוען מחדש עם האירוע והלקוח
        return _mapper.Map<DeletionRequestDto>(saved!);                 // ממיר
    }

    public async Task<List<DeletionRequestDto>> GetAsync(DeletionRequestStatus? status, CancellationToken ct)
    {
        var list = await _requests.GetListAsync(status, ct);            // שולף (אולי מסונן)
        return _mapper.Map<List<DeletionRequestDto>>(list);             // ממיר
    }

    public async Task ApproveAsync(int id, CancellationToken ct)
    {
        var request = await _requests.GetByIdAsync(id, ct)              // הבקשה (במעקב, עם האירוע)
                      ?? throw new NotFoundException("הבקשה לא נמצאה"); // 404

        if (request.Status != DeletionRequestStatus.Pending)            // כבר טופלה
            throw new ConflictException("הבקשה כבר טופלה");             // 409

        var day = await _calendar.GetForUpdateAsync(request.Event.Date, ct);    // היום של האירוע (במעקב)
        if (day != null)                                                // הגנה: אמור תמיד להתקיים
            day.IsTaken = false;                                        // משחרר את היום (EF יחליף Version בשמירה)

        var providerUserIds = await _items.GetAssignedProviderUserIdsAsync(request.EventId, ct);   // הספקים ששובצו (לפני שהסעיפים נמחקים)
        foreach (var userId in providerUserIds)                         // התראה לכל ספק
            _notifications.Add(new Notification                         // התראה חדשה
            {
                UserId = userId,                                        // הנמען
                Message = $"האירוע בתאריך {request.Event.Date:dd/MM/yyyy} ב{request.Event.Location} בוטל",    // הטקסט
                CreatedAtUtc = DateTime.UtcNow                          // זמן יצירה (IsRead: false כברירת מחדל)
            });

        _events.Remove(request.Event);                                  // מסמן מחיקת האירוע (סעיפים, מועמדויות והבקשה נמחקים ב-Cascade)
        await _uow.SaveChangesAsync(ct);                                // שמירה אחת: שחרור היום ומחיקת האירוע (או הכול או כלום)
    }

    public async Task<DeletionRequestDto> RejectAsync(int id, RejectDeletionRequestDto dto, CancellationToken ct)
    {
        var request = await _requests.GetByIdAsync(id, ct)              // הבקשה (במעקב)
                      ?? throw new NotFoundException("הבקשה לא נמצאה"); // 404

        if (request.Status != DeletionRequestStatus.Pending)            // כבר טופלה
            throw new ConflictException("הבקשה כבר טופלה");             // 409

        request.Status = DeletionRequestStatus.Rejected;                // סטטוס חדש
        request.AdminNote = dto.AdminNote?.Trim();                      // הערת האדמין (או null)
        request.ResolvedAtUtc = DateTime.UtcNow;                        // זמן הטיפול

        await _uow.SaveChangesAsync(ct);                                // שומר
        return _mapper.Map<DeletionRequestDto>(request);                // מחזיר את המעודכן
    }
}
