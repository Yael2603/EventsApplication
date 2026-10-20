using EventsApp.Application.DTOs.Calendar;

namespace EventsApp.Application.Services;

public interface ICalendarService
{
    Task<List<CalendarDayDto>> GetDaysAsync(DateOnly? from, DateOnly? to, CancellationToken ct);   // טווח אופציונלי
}
