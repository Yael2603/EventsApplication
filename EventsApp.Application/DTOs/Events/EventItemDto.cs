namespace EventsApp.Application.DTOs.Events;        // המקום הלוגי של המחלקה

public class EventItemDto                           // סעיף כפי שהוא חוזר ללקוח
{
    public int Id { get; set; }                     // מזהה הסעיף
    public int CategoryId { get; set; }             // מספר הקטגוריה
    public string CategoryName { get; set; } = string.Empty;    // שם הקטגוריה להצגה
    public decimal Budget { get; set; }             // התקציב
    public string? Details { get; set; }            // פרטים
    public int? ProviderId { get; set; }            // הספק שנבחר (null עד שהאדמין בוחר)
    public string? ProviderName { get; set; }       // שם העסק של הספק (null אם עוד לא נבחר)
}
