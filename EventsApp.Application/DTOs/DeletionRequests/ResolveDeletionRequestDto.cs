using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.DeletionRequests;

public enum DeletionDecision                        // ההחלטה של האדמין על הבקשה
{
    Approve,                                        // אישור: האירוע נמחק והיום משתחרר
    Reject                                          // דחייה: הבקשה נשמרת עם סטטוס Rejected
}

public class ResolveDeletionRequestDto              // גוף הבקשה ל-PATCH /api/deletion-requests/{id}
{
    [Required]                                      // חובה לשלוח החלטה
    [EnumDataType(typeof(DeletionDecision))]        // חייב להיות Approve או Reject; אחרת 400
    public DeletionDecision? Decision { get; set; } // nullable כדי ש-[Required] יזהה גוף בלי החלטה

    [MaxLength(1000)]                               // עד 1000 תווים
    public string? AdminNote { get; set; }          // הסבר ללקוח (רלוונטי לדחייה, אופציונלי)
}
