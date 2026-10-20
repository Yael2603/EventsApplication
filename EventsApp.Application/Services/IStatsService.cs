using EventsApp.Application.DTOs.Stats;

namespace EventsApp.Application.Services;

public interface IStatsService
{
    Task<AdminStatsDto> GetAsync(CancellationToken ct);     // מספרים ללוח בקרה
}
