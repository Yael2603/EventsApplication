using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.Notifications;

public class UpdateNotificationDto                  // גוף הבקשה ל-PATCH /api/notifications/{id}
{
    [Required]                                      // חובה לשלוח את השדה
    public bool? IsRead { get; set; }               // המצב החדש של ההתראה (true = נקראה)
}
