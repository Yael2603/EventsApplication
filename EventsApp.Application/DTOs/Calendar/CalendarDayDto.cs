namespace EventsApp.Application.DTOs.Calendar;      // המקום הלוגי של המחלקה

public class CalendarDayDto                         // יום בלוח, כפי שהלקוח רואה אותו
{
    public DateOnly Date { get; set; }              // התאריך (בלי שעה)
    public bool IsTaken { get; set; }               // true = כבר נתפס על ידי אירוע (Version לא נחשף)
}
