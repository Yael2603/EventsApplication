using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.Events;

public class AssignProviderDto                      // גוף הבקשה לשיבוץ ספק
{
    [Range(1, int.MaxValue)]                        // מספר ספק חיובי
    public int ProviderId { get; set; }             // הספק שנבחר
}
