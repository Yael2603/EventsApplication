using EventsApp.Domain.Enums;

namespace EventsApp.Application.DTOs.Events;        // המקום הלוגי של המחלקה

public class EventDto                               // אירוע כפי שהוא חוזר ללקוח
{
    public int Id { get; set; }                     // מזהה האירוע
    public int ClientId { get; set; }               // בעל האירוע
    public string ClientName { get; set; } = string.Empty;      // שם הלקוח (שימושי לאדמין)
    public DateOnly Date { get; set; }              // תאריך האירוע
    public EventType Type { get; set; }             // סוג האירוע
    public string Location { get; set; } = string.Empty;        // מקום
    public int GuestCount { get; set; }             // אורחים
    public string? Notes { get; set; }              // הערות
    public DateTime CreatedAtUtc { get; set; }      // מתי נוצר
    public List<EventItemDto> Items { get; set; } = new();      // הסעיפים
}
