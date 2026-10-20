namespace EventsApp.Application.DTOs.Stats;

public class AdminStatsDto                          // מספרים ללוח בקרה של האדמין
{
    public int TotalEvents { get; set; }            // כל האירועים
    public int UpcomingEvents { get; set; }         // אירועים עתידיים
    public int PendingProviders { get; set; }       // ספקים שממתינים לאישור
    public int ApprovedProviders { get; set; }      // ספקים מאושרים
    public int PendingDeletionRequests { get; set; }    // בקשות מחיקה ממתינות
    public int OpenItems { get; set; }              // סעיפים עתידיים בלי ספק
}
