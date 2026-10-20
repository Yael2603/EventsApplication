namespace EventsApp.Application.DTOs.Candidacies;

public class CandidacyDto                           // מועמדות כפי שהאדמין רואה אותה
{
    public int Id { get; set; }                     // מזהה המועמדות
    public int EventItemId { get; set; }            // הסעיף
    public int ProviderId { get; set; }             // הספק (לשימוש בשיבוץ)
    public string ProviderName { get; set; } = string.Empty;    // שם העסק
    public string Phone { get; set; } = string.Empty;           // טלפון הספק
    public string City { get; set; } = string.Empty;            // עיר הספק
    public string? Message { get; set; }            // ההודעה שהספק צירף
    public decimal ProposedPrice { get; set; }      // המחיר המוצע
    public DateTime CreatedAtUtc { get; set; }      // מתי הוגשה
}
