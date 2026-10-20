using EventsApp.Application.DTOs.Stats;
using EventsApp.Application.Repositories;
using EventsApp.Domain.Enums;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class StatsRepository : IStatsRepository
{
    private readonly AppDbContext _db;
    public StatsRepository(AppDbContext db) { _db = db; }       // הזרקה

    public async Task<AdminStatsDto> GetAsync(DateOnly today, CancellationToken ct) => new AdminStatsDto
    {
        TotalEvents = await _db.Events.CountAsync(ct),                                              // כל האירועים
        UpcomingEvents = await _db.Events.CountAsync(e => e.Date > today, ct),                      // עתידיים
        PendingProviders = await _db.Providers.CountAsync(p => p.Status == ProviderStatus.Pending, ct),    // ממתינים
        ApprovedProviders = await _db.Providers.CountAsync(p => p.Status == ProviderStatus.Approved, ct),  // מאושרים
        PendingDeletionRequests = await _db.DeletionRequests.CountAsync(r => r.Status == DeletionRequestStatus.Pending, ct),   // בקשות ממתינות
        OpenItems = await _db.EventItems.CountAsync(i => i.ProviderId == null && i.Event.Date > today, ct)  // סעיפים עתידיים בלי ספק
    };
}
