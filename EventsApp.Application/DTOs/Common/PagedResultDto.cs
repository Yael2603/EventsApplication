namespace EventsApp.Application.DTOs.Common;

public class PagedResultDto<T>                      // תשובה של עמוד אחד מתוך רשימה ארוכה (גנרי: מתאים לכל סוג DTO)
{
    public List<T> Items { get; set; } = new();     // השורות של העמוד הנוכחי בלבד
    public int Page { get; set; }                   // מספר העמוד שהוחזר
    public int PageSize { get; set; }               // כמה שורות בעמוד
    public int TotalCount { get; set; }             // כמה שורות יש בסך הכול בבסיס הנתונים
    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);  // כמה עמודים יש (מעוגל למעלה)
}
