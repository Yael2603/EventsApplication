using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.DeletionRequests;

public class CreateDeletionRequestDto               // גוף בקשת המחיקה
{
    [Required, MinLength(10), MaxLength(1000)]      // חובה, 10 עד 1000 תווים (נימוק אמיתי, לא "כי")
    public string Reason { get; set; } = string.Empty;  // הנימוק
}
