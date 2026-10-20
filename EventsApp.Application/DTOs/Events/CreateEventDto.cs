using System.ComponentModel.DataAnnotations;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.DTOs.Events;        // המקום הלוגי של המחלקה

public class CreateEventDto                         // טופס יצירת אירוע
{
    public DateOnly Date { get; set; }              // תאריך האירוע (פורמט 2026-12-01); נבדק בשירות

    [EnumDataType(typeof(EventType))]               // חייב להיות ערך קיים ב-Enum (לא מספר שרירותי)
    public EventType Type { get; set; }             // סוג האירוע (חתונה, בר מצווה ...)

    [Required, MaxLength(200)]                      // חובה, עד 200 תווים
    public string Location { get; set; } = string.Empty;    // מקום האירוע

    [Range(1, 5000)]                                // מספר אורחים הגיוני
    public int GuestCount { get; set; }             // מספר אורחים

    [MaxLength(1000)]                               // עד 1000 תווים
    public string? Notes { get; set; }              // הערות (אופציונלי)

    [Required, MinLength(1)]                        // חובה סעיף אחד לפחות
    public List<CreateEventItemDto> Items { get; set; } = new();    // סעיפי האירוע; ה-[ApiController] בודק גם כל סעיף בנפרד
}
