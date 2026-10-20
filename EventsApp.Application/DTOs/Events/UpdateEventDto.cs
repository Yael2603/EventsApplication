using System.ComponentModel.DataAnnotations;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.DTOs.Events;

public class UpdateEventDto                         // עריכת אירוע (בלי תאריך ובלי סעיפים)
{
    [EnumDataType(typeof(EventType))]               // חייב להיות ערך קיים
    public EventType Type { get; set; }             // סוג האירוע
    [Required, MaxLength(200)]                      // חובה, עד 200 תווים
    public string Location { get; set; } = string.Empty;    // מקום
    [Range(1, 5000)]                                // מספר אורחים הגיוני
    public int GuestCount { get; set; }             // אורחים
    [MaxLength(1000)]                               // עד 1000 תווים
    public string? Notes { get; set; }              // הערות
}
