using EventsApp.Application.DTOs.Stats;
using EventsApp.Application.Repositories;

namespace EventsApp.Application.Services;

public class StatsService : IStatsService
{
    private readonly IStatsRepository _stats;           // ספירות
    public StatsService(IStatsRepository stats) { _stats = stats; }     // הזרקה

    public Task<AdminStatsDto> GetAsync(CancellationToken ct) =>
        _stats.GetAsync(DateOnly.FromDateTime(DateTime.UtcNow), ct);    // "היום" נקבע בשירות, והשאילתה בשכבת הנתונים
}
