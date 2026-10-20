using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.Candidacies;

public class CreateCandidacyDto                     // גוף הבקשה להגשת מועמדות
{
    [Range(1, int.MaxValue)]                        // מספר סעיף חיובי
    public int EventItemId { get; set; }            // הסעיף שאליו מגישים
    [MaxLength(1000)]                               // עד 1000 תווים
    public string? Message { get; set; }            // הודעה ללקוח או לאדמין (אופציונלי)
    [Range(0.01, 10000000.0)]                       // מחיר חיובי וסביר
    public decimal ProposedPrice { get; set; }      // המחיר המוצע
}
