using AutoMapper;
using EventsApp.Application.Abstractions;
using EventsApp.Application.DTOs.Events;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _events;          // אירועים
    private readonly ICalendarRepository _calendar;     // לוח
    private readonly ICategoryRepository _categories;   // לבדיקת קטגוריות
    private readonly IEventItemRepository _items;       // חדש: סעיפים
    private readonly IProviderRepository _providers;    // חדש: ספקים
    private readonly IUnitOfWork _uow;                  // שמירה אחת
    private readonly IMapper _mapper;                   // המרה

    public EventService(IEventRepository events, ICalendarRepository calendar,
        ICategoryRepository categories, IEventItemRepository items,
        IProviderRepository providers, IUnitOfWork uow, IMapper mapper)   // הזרקה (שתי תלויות חדשות)
    {
        _events = events;
        _calendar = calendar;
        _categories = categories;
        _items = items;
        _providers = providers;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<EventDto> CreateAsync(int clientId, CreateEventDto dto, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);             // היום
        if (dto.Date <= today)                                          // לא ניתן להזמין להיום או לעבר
            throw new BadRequestException("ניתן להזמין תאריך עתידי בלבד");   // 400

        var categoryIds = dto.Items.Select(i => i.CategoryId).ToList(); // כל הקטגוריות שנבחרו
        if (categoryIds.Distinct().Count() != categoryIds.Count)        // אם קטגוריה חוזרת פעמיים
            throw new BadRequestException("ניתן להוסיף סעיף אחד בלבד לכל קטגוריה");     // 400

        foreach (var categoryId in categoryIds)                         // בודק שכל קטגוריה קיימת
            if (!await _categories.ExistsAsync(categoryId, ct))
                throw new BadRequestException($"הקטגוריה {categoryId} אינה קיימת");     // 400

        var day = await _calendar.GetForUpdateAsync(dto.Date, ct)       // היום מהלוח, עם מעקב
                  ?? throw new BadRequestException("התאריך אינו קיים בלוח החברה");      // 400 אם אין

        if (day.IsTaken)                                                // כבר תפוס
            throw new ConflictException("התאריך כבר תפוס. יש לבחור תאריך אחר");        // 409

        day.IsTaken = true;                                             // תופס את היום (EF יחליף Version בשמירה)

        var ev = _mapper.Map<Event>(dto);                               // יוצר Event עם הסעיפים מה-DTO
        ev.ClientId = clientId;                                         // בעל האירוע: מהטוקן, לא מהבקשה
        ev.CreatedAtUtc = DateTime.UtcNow;                              // זמן יצירה

        _events.Add(ev);                                                // מסמן להוספה
        await _uow.SaveChangesAsync(ct);                                // שמירה אחת: תפיסת היום והאירוע. אם נכשל: 409

        var created = await _events.GetByIdAsync(ev.Id, ct);            // טוען מחדש עם שמות קטגוריות
        return _mapper.Map<EventDto>(created!);                         // ממיר ל-DTO
    }

    public async Task<List<EventDto>> GetMineAsync(int clientId, CancellationToken ct)
    {
        var items = await _events.GetByClientAsync(clientId, ct);       // האירועים של הלקוח
        return _mapper.Map<List<EventDto>>(items);                      // ממיר
    }

    public async Task<EventDto> GetByIdAsync(int id, int userId, bool isAdmin, CancellationToken ct)
    {
        var ev = await _events.GetByIdAsync(id, ct)                     // מחפש
                 ?? throw new NotFoundException("האירוע לא נמצא");      // 404

        if (!isAdmin && ev.ClientId != userId)                          // לא אדמין ולא הבעלים
            throw new ForbiddenException("אין הרשאה לצפות באירוע זה");  // 403

        return _mapper.Map<EventDto>(ev);                               // ממיר
    }

    public async Task<List<EventDto>> GetAllAsync(CancellationToken ct)
    {
        var items = await _events.GetAllAsync(ct);                      // כולם
        return _mapper.Map<List<EventDto>>(items);                      // ממיר
    }

    public async Task<EventItemDto> AssignProviderAsync(int itemId, AssignProviderDto dto, CancellationToken ct)
    {
        var item = await _items.GetByIdAsync(itemId, ct)                // הסעיף (במעקב, עם האירוע)
                   ?? throw new NotFoundException("הסעיף לא נמצא");     // 404

        if (item.Event.Date <= DateOnly.FromDateTime(DateTime.UtcNow))  // אירוע שעבר
            throw new BadRequestException("לא ניתן לשבץ ספק לאירוע שהתקיים");   // 400

        var provider = await _providers.GetByIdAsync(dto.ProviderId, ct)    // הספק שנבחר (גם אם לא הגיש מועמדות)
                       ?? throw new NotFoundException("הספק לא נמצא");      // 404

        if (provider.Status != ProviderStatus.Approved)                 // ספק לא מאושר
            throw new BadRequestException("ניתן לשבץ רק ספק מאושר");    // 400
        if (provider.CategoryId != item.CategoryId)                     // קטגוריה לא תואמת
            throw new BadRequestException("הספק אינו בקטגוריה של הסעיף");   // 400

        item.Provider = provider;                                       // שיבוץ (EF יעדכן ProviderId; מותר גם שיבוץ מחדש)
        await _uow.SaveChangesAsync(ct);                                // שומר
        return _mapper.Map<EventItemDto>(item);                         // מחזיר את הסעיף המעודכן, כולל שם הספק
    }

    public async Task<EventDto> UpdateAsync(int clientId, int id, UpdateEventDto dto, CancellationToken ct)
    {
        var ev = await _events.GetForUpdateAsync(id, ct)                // האירוע במעקב
                 ?? throw new NotFoundException("האירוע לא נמצא");      // 404
        if (ev.ClientId != clientId)                                    // לא הבעלים
            throw new ForbiddenException("ניתן לערוך רק אירוע שלך");    // 403
        if (ev.Date <= DateOnly.FromDateTime(DateTime.UtcNow))          // אירוע שעבר
            throw new BadRequestException("לא ניתן לערוך אירוע שהתקיים");   // 400

        ev.Type = dto.Type;                                             // סוג
        ev.Location = dto.Location.Trim();                              // מקום
        ev.GuestCount = dto.GuestCount;                                 // אורחים
        ev.Notes = dto.Notes?.Trim();                                   // הערות (או null)
        // בכוונה לא נוגעים בתאריך (דורש החלפת יום בלוח)

        await _uow.SaveChangesAsync(ct);                                // שומר
        var updated = await _events.GetByIdAsync(id, ct);               // טוען מחדש עם כל הפרטים
        return _mapper.Map<EventDto>(updated!);                         // ממיר
    }

    public async Task<EventItemDto> AddItemAsync(int clientId, int eventId, CreateEventItemDto dto, CancellationToken ct)
    {
        var ev = await _events.GetForUpdateAsync(eventId, ct)           // האירוע במעקב, עם הסעיפים
                 ?? throw new NotFoundException("האירוע לא נמצא");      // 404
        if (ev.ClientId != clientId)                                    // לא הבעלים
            throw new ForbiddenException("ניתן לערוך רק אירוע שלך");    // 403
        if (ev.Date <= DateOnly.FromDateTime(DateTime.UtcNow))          // אירוע שעבר
            throw new BadRequestException("לא ניתן לערוך אירוע שהתקיים");   // 400
        if (!await _categories.ExistsAsync(dto.CategoryId, ct))         // קטגוריה לא קיימת
            throw new BadRequestException("הקטגוריה אינה קיימת");       // 400
        if (ev.Items.Any(i => i.CategoryId == dto.CategoryId))          // כבר יש סעיף בקטגוריה
            throw new ConflictException("כבר קיים באירוע סעיף בקטגוריה זו");    // 409

        var item = _mapper.Map<EventItem>(dto);                         // סעיף חדש מה-DTO
        ev.Items.Add(item);                                             // מוסיף לאירוע (EF ימלא EventId)
        await _uow.SaveChangesAsync(ct);                                // שומר

        var saved = await _items.GetByIdAsync(item.Id, ct);             // טוען עם שם הקטגוריה
        return _mapper.Map<EventItemDto>(saved!);                       // ממיר
    }

    public async Task RemoveItemAsync(int clientId, int itemId, CancellationToken ct)
    {
        var item = await _items.GetByIdAsync(itemId, ct)                // הסעיף (במעקב, עם האירוע)
                   ?? throw new NotFoundException("הסעיף לא נמצא");     // 404
        if (item.Event.ClientId != clientId)                            // לא הבעלים
            throw new ForbiddenException("ניתן לערוך רק אירוע שלך");    // 403
        if (item.Event.Date <= DateOnly.FromDateTime(DateTime.UtcNow))  // אירוע שעבר
            throw new BadRequestException("לא ניתן לערוך אירוע שהתקיים");   // 400
        if (item.ProviderId != null)                                    // כבר שובץ ספק
            throw new ConflictException("לא ניתן להסיר סעיף ששובץ בו ספק");     // 409
        if (await _items.CountByEventAsync(item.EventId, ct) <= 1)      // הסעיף האחרון
            throw new BadRequestException("חייב להישאר באירוע סעיף אחד לפחות"); // 400

        _items.Remove(item);                                            // מסמן מחיקה
        await _uow.SaveChangesAsync(ct);                                // שומר
    }
}
