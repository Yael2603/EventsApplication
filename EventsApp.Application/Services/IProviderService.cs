using EventsApp.Application.DTOs.Providers;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.Services;

public interface IProviderService
{
    Task<List<ProviderDto>> GetAsync(ProviderStatus? status, CancellationToken ct);                 // רשימה לאדמין
    Task<ProviderDto> UpdateStatusAsync(int id, UpdateProviderStatusDto dto, CancellationToken ct); // אישור או דחייה
    Task<ProviderDto> GetMineAsync(int userId, CancellationToken ct);                               // הפרופיל שלי
    Task<ProviderDto> UpdateMineAsync(int userId, UpdateProviderProfileDto dto, CancellationToken ct);  // עדכון הפרופיל שלי
    Task<List<BookingDto>> GetBookingsAsync(int userId, CancellationToken ct);                      // השיבוצים שלי
}
