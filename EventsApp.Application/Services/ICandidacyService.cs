using EventsApp.Application.DTOs.Candidacies;
using EventsApp.Application.DTOs.Events;

namespace EventsApp.Application.Services;

public interface ICandidacyService
{
    Task<List<OpenItemDto>> GetOpenItemsAsync(int userId, CancellationToken ct);            // סעיפים פתוחים לספק המחובר
    Task<CandidacyDto> ApplyAsync(int userId, CreateCandidacyDto dto, CancellationToken ct);// הגשת מועמדות
    Task<List<CandidacyDto>> GetByItemAsync(int eventItemId, CancellationToken ct);         // מועמדים לסעיף (אדמין)
    Task<List<MyCandidacyDto>> GetMineAsync(int userId, CancellationToken ct);      // המועמדויות שלי
    Task WithdrawAsync(int userId, int candidacyId, CancellationToken ct);          // ביטול מועמדות
}
