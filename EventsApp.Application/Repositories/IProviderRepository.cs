using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.Repositories;

public interface IProviderRepository                // גישה לספקים
{
    Task<Provider?> GetByIdAsync(int id, CancellationToken ct);         // ספק במעקב EF עם משתמש וקטגוריה (לעדכון/שיבוץ)
    Task<Provider?> GetByUserIdAsync(int userId, CancellationToken ct); // הספק של המשתמש המחובר (במעקב)
    Task<List<Provider>> GetListAsync(ProviderStatus? status, CancellationToken ct);   // רשימה לקריאה בלבד, אופציונלית לפי סטטוס
}
