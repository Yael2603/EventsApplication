using EventsApp.Application.DTOs.DeletionRequests;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.Services;

public interface IDeletionRequestService
{
    Task<DeletionRequestDto> CreateAsync(int clientId, int eventId, CreateDeletionRequestDto dto, CancellationToken ct);    // לקוח מבקש
    Task<List<DeletionRequestDto>> GetAsync(DeletionRequestStatus? status, CancellationToken ct);       // אדמין: רשימה
    Task ApproveAsync(int id, CancellationToken ct);                                                    // אדמין: אישור (מחיקה)
    Task<DeletionRequestDto> RejectAsync(int id, RejectDeletionRequestDto dto, CancellationToken ct);   // אדמין: דחייה
}
