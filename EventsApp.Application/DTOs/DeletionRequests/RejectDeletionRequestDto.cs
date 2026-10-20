using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.DeletionRequests;

public class RejectDeletionRequestDto               // גוף הדחייה
{
    [MaxLength(1000)]                               // עד 1000 תווים
    public string? AdminNote { get; set; }          // הסבר ללקוח (אופציונלי)
}
