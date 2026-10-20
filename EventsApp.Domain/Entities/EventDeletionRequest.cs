using EventsApp.Domain.Enums;

namespace EventsApp.Domain.Entities;

public class EventDeletionRequest                   // בקשת לקוח למחוק אירוע
{
    public int Id { get; set; }                     // מפתח ראשי
    public int EventId { get; set; }                // האירוע שמבקשים למחוק (מפתח זר)
    public Event Event { get; set; } = null!;       // האירוע עצמו (ניווט)
    public string Reason { get; set; } = string.Empty;          // הנימוק של הלקוח (חובה)
    public DeletionRequestStatus Status { get; set; } = DeletionRequestStatus.Pending;  // מתחילה ממתינה
    public string? AdminNote { get; set; }          // הערת האדמין בדחייה (אופציונלי)
    public DateTime CreatedAtUtc { get; set; }      // מתי הוגשה
    public DateTime? ResolvedAtUtc { get; set; }    // מתי נדחתה (null כל עוד ממתינה)
}
