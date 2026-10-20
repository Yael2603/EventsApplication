using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.Events;        // המקום הלוגי של המחלקה

public class CreateEventItemDto                     // סעיף באירוע שהלקוח שולח (למשל קייטרינג עם תקציב)
{
    [Range(1, int.MaxValue)]                        // חייב להיות מספר קטגוריה חיובי
    public int CategoryId { get; set; }             // הקטגוריה (נשלח id מהרשימה)

    [Range(0.01, 10000000.0)]                       // תקציב חיובי וסביר
    public decimal Budget { get; set; }             // התקציב המבוקש לסעיף

    [MaxLength(1000)]                               // עד 1000 תווים
    public string? Details { get; set; }            // פרטים חופשיים (אופציונלי)
}
