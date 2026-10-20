using EventsApp.Domain.Enums;

namespace EventsApp.Application.DTOs.Events;

public class OpenItemDto                            // סעיף פתוח כפי שספק רואה אותו (בלי פרטי הלקוח)
{
    public int ItemId { get; set; }                 // מזהה הסעיף (נשלח בהגשת מועמדות)
    public int EventId { get; set; }                // מזהה האירוע
    public DateOnly EventDate { get; set; }         // תאריך האירוע
    public EventType EventType { get; set; }        // סוג האירוע
    public string Location { get; set; } = string.Empty;        // מקום
    public int GuestCount { get; set; }             // אורחים
    public string CategoryName { get; set; } = string.Empty;    // שם הקטגוריה
    public decimal Budget { get; set; }             // תקציב הלקוח לסעיף
    public string? Details { get; set; }            // פרטי הלקוח לסעיף
    public bool AlreadyApplied { get; set; }        // האם הספק כבר הגיש לסעיף הזה
}
