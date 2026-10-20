using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.Providers;

public class UpdateProviderProfileDto               // גוף הבקשה לעדכון פרופיל הספק (בלי קטגוריה ובלי סטטוס)
{
    [Required, MaxLength(150)]                      // חובה, עד 150 תווים
    public string BusinessName { get; set; } = string.Empty;    // שם העסק
    [MaxLength(1000)]                               // עד 1000 תווים
    public string? Description { get; set; }        // תיאור (אופציונלי)
    [Required, MaxLength(30)]                       // חובה, עד 30 תווים
    public string Phone { get; set; } = string.Empty;           // טלפון
    [Required, MaxLength(100)]                      // חובה, עד 100 תווים
    public string City { get; set; } = string.Empty;            // עיר
    [Range(0.01, 10000000.0)]                       // מחיר חיובי וסביר
    public decimal StartingPrice { get; set; }      // מחיר התחלתי
}
