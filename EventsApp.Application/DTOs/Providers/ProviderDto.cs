using EventsApp.Domain.Enums;

namespace EventsApp.Application.DTOs.Providers;

public class ProviderDto                            // ספק כפי שהאדמין רואה אותו
{
    public int Id { get; set; }                     // מזהה הספק (לא מזהה המשתמש)
    public string BusinessName { get; set; } = string.Empty;    // שם העסק
    public string Description { get; set; } = string.Empty;     // תיאור
    public string Phone { get; set; } = string.Empty;           // טלפון ליצירת קשר
    public string City { get; set; } = string.Empty;            // עיר
    public int CategoryId { get; set; }             // מספר הקטגוריה
    public string CategoryName { get; set; } = string.Empty;    // שם הקטגוריה להצגה
    public decimal StartingPrice { get; set; }      // מחיר התחלתי
    public ProviderStatus Status { get; set; }      // ממתין / מאושר / נדחה
    public string Email { get; set; } = string.Empty;           // אימייל (מהמשתמש המקושר)
    public DateTime AcceptedTermsAt { get; set; }   // מתי אישר את התנאים
}
