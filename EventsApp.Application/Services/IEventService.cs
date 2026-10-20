using EventsApp.Application.DTOs.Events;

namespace EventsApp.Application.Services;

public interface IEventService
{
    Task<EventDto> CreateAsync(int clientId, CreateEventDto dto, CancellationToken ct);     // יצירת אירוע ותפיסת יום
    Task<List<EventDto>> GetMineAsync(int clientId, CancellationToken ct);                  // האירועים שלי
    Task<EventDto> GetByIdAsync(int id, int userId, bool isAdmin, CancellationToken ct);    // אירוע, עם בדיקת בעלות
    Task<List<EventDto>> GetAllAsync(CancellationToken ct);                                 // כל האירועים (אדמין)
    Task<EventItemDto> AssignProviderAsync(int itemId, AssignProviderDto dto, CancellationToken ct);   // שיבוץ ספק בסעיף (אדמין)
    Task<EventDto> UpdateAsync(int clientId, int id, UpdateEventDto dto, CancellationToken ct);     // עריכת אירוע
    Task<EventItemDto> AddItemAsync(int clientId, int eventId, CreateEventItemDto dto, CancellationToken ct);   // הוספת סעיף
    Task RemoveItemAsync(int clientId, int itemId, CancellationToken ct);                           // הסרת סעיף
}
