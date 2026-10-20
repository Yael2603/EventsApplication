using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class CalendarRepository : ICalendarRepository
{
    private readonly AppDbContext _db;
    public CalendarRepository(AppDbContext db) { _db = db; }    // הזרקה

    public Task<List<CompanyDay>> GetRangeAsync(DateOnly from, DateOnly to, CancellationToken ct) =>
        _db.CompanyDays.AsNoTracking()              // קריאה בלבד, בלי מעקב (מהיר יותר)
            .Where(d => d.Date >= from && d.Date <= to)     // רק הטווח המבוקש
            .OrderBy(d => d.Date)                   // ממוין לפי תאריך
            .ToListAsync(ct);                       // מריץ ומחזיר רשימה

    public Task<CompanyDay?> GetForUpdateAsync(DateOnly date, CancellationToken ct) =>
        _db.CompanyDays.FirstOrDefaultAsync(d => d.Date == date, ct);   // עם מעקב, כדי ש-EF ידע לעדכן ולבדוק Version
}
