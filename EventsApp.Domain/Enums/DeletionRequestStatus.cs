namespace EventsApp.Domain.Enums;

public enum DeletionRequestStatus                   // מצבי בקשת מחיקה
{
    Pending,                                        // ממתינה לאדמין (נשמרת ב-DB כמחרוזת "Pending")
    Rejected                                        // נדחתה (אישור = מחיקת האירוע והבקשה, ולכן אין סטטוס כזה)
}
