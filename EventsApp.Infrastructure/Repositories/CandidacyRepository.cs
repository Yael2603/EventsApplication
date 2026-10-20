using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class CandidacyRepository : ICandidacyRepository
{
    private readonly AppDbContext _db;
    public CandidacyRepository(AppDbContext db) { _db = db; }   // הזרקה

    public void Add(Candidacy candidacy) => _db.Candidacies.Add(candidacy);     // רק מסמן הוספה

    public Task<bool> ExistsAsync(int eventItemId, int providerId, CancellationToken ct) =>
        _db.Candidacies.AnyAsync(c => c.EventItemId == eventItemId && c.ProviderId == providerId, ct);  // בדיקה בלי לטעון שורות

    public Task<List<Candidacy>> GetByItemAsync(int eventItemId, CancellationToken ct) =>
        _db.Candidacies.AsNoTracking().Include(c => c.Provider)         // קריאה בלבד, עם פרטי הספק
            .Where(c => c.EventItemId == eventItemId)                   // רק של הסעיף
            .OrderBy(c => c.CreatedAtUtc).ToListAsync(ct);              // לפי סדר ההגשה

    public Task<List<Candidacy>> GetByProviderAsync(int providerId, CancellationToken ct) =>
        _db.Candidacies.AsNoTracking()                                          // קריאה בלבד
            .Include(c => c.EventItem).ThenInclude(i => i.Event)                // תאריך, סוג ומקום
            .Include(c => c.EventItem).ThenInclude(i => i.Category)             // שם קטגוריה
            .Where(c => c.ProviderId == providerId)                             // רק של הספק
            .OrderByDescending(c => c.CreatedAtUtc).ToListAsync(ct);            // החדשות קודם

    public Task<Candidacy?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Candidacies.Include(c => c.EventItem)                               // במעקב, עם הסעיף (לבדיקת שיבוץ)
            .FirstOrDefaultAsync(c => c.Id == id, ct);                          // אחת או null

    public void Remove(Candidacy candidacy) => _db.Candidacies.Remove(candidacy);   // רק מסמן מחיקה
}
