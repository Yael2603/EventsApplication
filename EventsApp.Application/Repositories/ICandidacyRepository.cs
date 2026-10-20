using EventsApp.Domain.Entities;

namespace EventsApp.Application.Repositories;

public interface ICandidacyRepository               // גישה למועמדויות
{
    void Add(Candidacy candidacy);                  // מסמן הוספה, בלי שמירה (השמירה ב-IUnitOfWork)
    Task<bool> ExistsAsync(int eventItemId, int providerId, CancellationToken ct);      // האם כבר הוגשה
    Task<List<Candidacy>> GetByItemAsync(int eventItemId, CancellationToken ct);        // כל המועמדים לסעיף
    Task<List<Candidacy>> GetByProviderAsync(int providerId, CancellationToken ct);     // חדש: מועמדויות של ספק
    Task<Candidacy?> GetByIdAsync(int id, CancellationToken ct);                        // חדש: מועמדות במעקב עם הסעיף
    void Remove(Candidacy candidacy);                                                   // חדש: מסמן מחיקה
}
