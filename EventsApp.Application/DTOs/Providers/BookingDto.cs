using EventsApp.Domain.Enums;

namespace EventsApp.Application.DTOs.Providers;

public class BookingDto                             // סעיף ששובץ לספק
{
    public int ItemId { get; set; }                 // מזהה הסעיף
    public int EventId { get; set; }                // מזהה האירוע
    public DateOnly EventDate { get; set; }         // תאריך
    public EventType EventType { get; set; }        // סוג האירוע
    public string Location { get; set; } = string.Empty;        // מקום
    public int GuestCount { get; set; }             // אורחים
    public string CategoryName { get; set; } = string.Empty;    // קטגוריה
    public decimal Budget { get; set; }             // תקציב הלקוח
    public string? Details { get; set; }            // פרטי הסעיף
    public string ClientName { get; set; } = string.Empty;      // שם הלקוח
    public string ClientEmail { get; set; } = string.Empty;     // אימייל הלקוח
}
