using AutoMapper;
using EventsApp.Application.DTOs.Calendar;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;

namespace EventsApp.Application.Services;

public class CalendarService : ICalendarService
{
    private readonly ICalendarRepository _calendar;     // גישה ללוח
    private readonly IMapper _mapper;                   // המרה

    public CalendarService(ICalendarRepository calendar, IMapper mapper)    // הזרקה
    {
        _calendar = calendar;
        _mapper = mapper;
    }

    public async Task<List<CalendarDayDto>> GetDaysAsync(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var start = from ?? DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);   // ברירת מחדל: מחר
        var end = to ?? start.AddDays(60);              // ברירת מחדל: 60 יום קדימה

        if (end < start)                                // טווח הפוך
            throw new BadRequestException("תאריך הסיום חייב להיות אחרי תאריך ההתחלה");   // 400

        if (end.DayNumber - start.DayNumber > 400)      // הגנה מפני בקשת ענק
            throw new BadRequestException("ניתן לבקש עד 400 ימים בבת אחת");              // 400

        var days = await _calendar.GetRangeAsync(start, end, ct);       // שולף מה-DB
        return _mapper.Map<List<CalendarDayDto>>(days);                 // ממיר ל-DTOs
    }
}
