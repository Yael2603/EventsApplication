namespace EventsApp.Application.DTOs.Notifications;

public class NotificationDto                        // התראה כפי שהיא חוזרת
{
    public int Id { get; set; }                     // מזהה
    public string Message { get; set; } = string.Empty;     // הטקסט
    public bool IsRead { get; set; }                // נקראה?
    public DateTime CreatedAtUtc { get; set; }      // מתי נוצרה
}
