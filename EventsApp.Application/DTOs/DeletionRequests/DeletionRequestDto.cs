using EventsApp.Domain.Enums;

namespace EventsApp.Application.DTOs.DeletionRequests;

public class DeletionRequestDto                     // בקשה כפי שהיא חוזרת
{
    public int Id { get; set; }                     // מזהה הבקשה
    public int EventId { get; set; }                // האירוע
    public DateOnly EventDate { get; set; }         // תאריך האירוע
    public EventType EventType { get; set; }        // סוג האירוע
    public string Location { get; set; } = string.Empty;        // מקום
    public int ClientId { get; set; }               // הלקוח
    public string ClientName { get; set; } = string.Empty;      // שם הלקוח
    public string Reason { get; set; } = string.Empty;          // הנימוק
    public DeletionRequestStatus Status { get; set; }           // ממתינה / נדחתה
    public string? AdminNote { get; set; }          // הערת האדמין
    public DateTime CreatedAtUtc { get; set; }      // מתי הוגשה
    public DateTime? ResolvedAtUtc { get; set; }    // מתי נדחתה
}
