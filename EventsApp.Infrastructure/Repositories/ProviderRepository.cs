using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class ProviderRepository : IProviderRepository
{
    private readonly AppDbContext _db;
    public ProviderRepository(AppDbContext db) { _db = db; }    // הזרקה

    public Task<Provider?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Providers.Include(p => p.User).Include(p => p.Category)     // במעקב (בלי AsNoTracking) כדי שעדכון Status יישמר
            .FirstOrDefaultAsync(p => p.Id == id, ct);                  // ספק אחד או null

    public Task<Provider?> GetByUserIdAsync(int userId, CancellationToken ct) =>
        _db.Providers.Include(p => p.User).Include(p => p.Category)     // חדש: טוען גם משתמש וקטגוריה (למיפוי לפרופיל)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);          // במעקב, כמו קודם

    public Task<List<Provider>> GetListAsync(ProviderStatus? status, CancellationToken ct) =>
        _db.Providers.AsNoTracking()                                    // קריאה בלבד
            .Include(p => p.User).Include(p => p.Category)              // לאימייל ולשם הקטגוריה
            .Where(p => status == null || p.Status == status)           // בלי סטטוס: כולם
            .OrderBy(p => p.Id).ToListAsync(ct);                        // לפי מזהה
}
