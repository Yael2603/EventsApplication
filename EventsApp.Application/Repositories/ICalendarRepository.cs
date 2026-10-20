using EventsApp.Domain.Entities;

namespace EventsApp.Application.Repositories;

public interface ICalendarRepository                // גישה ללוח החברה
{
    Task<List<CompanyDay>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken ct);   // ימים בטווח, לקריאה בלבד
    Task<CompanyDay?> GetForUpdateAsync(DateOnly date, CancellationToken ct);                // יום אחד במעקב EF, כדי לעדכן אותו
}
