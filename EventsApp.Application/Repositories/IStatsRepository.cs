using EventsApp.Application.DTOs.Stats;

namespace EventsApp.Application.Repositories;

public interface IStatsRepository                   // ספירות ללוח בקרה
{
    Task<AdminStatsDto> GetAsync(DateOnly today, CancellationToken ct);         // כל המספרים בבת אחת
}
