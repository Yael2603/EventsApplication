using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.Common;

public class PaginationQueryDto                     // פרמטרי עימוד שמגיעים מה-URL: ?page=1&pageSize=10
{
    public const int MaxPageSize = 100;             // תקרה, כדי שאי אפשר יהיה לבקש את כל הטבלה בבת אחת

    [Range(1, int.MaxValue)]                        // עמוד מתחיל ב-1; ערך קטן יותר מחזיר 400 אוטומטית ([ApiController])
    public int Page { get; set; } = 1;              // מספר העמוד המבוקש (ברירת מחדל: הראשון)

    [Range(1, MaxPageSize)]                         // בין 1 ל-100 שורות בעמוד; אחרת 400
    public int PageSize { get; set; } = 10;         // כמה שורות בעמוד (ברירת מחדל: 10)
}
