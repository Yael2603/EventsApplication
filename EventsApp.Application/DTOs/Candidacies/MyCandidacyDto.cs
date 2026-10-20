using EventsApp.Domain.Enums;

namespace EventsApp.Application.DTOs.Candidacies;

public class MyCandidacyDto                         // מועמדות כפי שהספק רואה אותה
{
    public int Id { get; set; }                     // מזהה (לביטול)
    public int EventItemId { get; set; }            // הסעיף
    public DateOnly EventDate { get; set; }         // תאריך האירוע
    public EventType EventType { get; set; }        // סוג האירוע
    public string Location { get; set; } = string.Empty;        // מקום
    public string CategoryName { get; set; } = string.Empty;    // קטגוריה
    public decimal ProposedPrice { get; set; }      // המחיר שהוצע
    public string? Message { get; set; }            // ההודעה שנשלחה
    public DateTime CreatedAtUtc { get; set; }      // מתי הוגשה
    public CandidacyOutcome Outcome { get; set; }   // פתוח / זכיתי / הפסדתי
}
