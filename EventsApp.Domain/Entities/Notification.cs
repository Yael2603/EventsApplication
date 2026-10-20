namespace EventsApp.Domain.Entities;

public class Notification                           // התראה למשתמש
{
    public int Id { get; set; }                     // מפתח ראשי
    public int UserId { get; set; }                 // מי מקבל (מפתח זר)
    public User User { get; set; } = null!;         // המשתמש (ניווט)
    public string Message { get; set; } = string.Empty;     // הטקסט
    public bool IsRead { get; set; }                // נקראה? (ברירת מחדל false)
    public DateTime CreatedAtUtc { get; set; }      // מתי נוצרה
}
