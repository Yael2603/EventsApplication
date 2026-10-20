using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Domain.Enums;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class DeletionRequestRepository : IDeletionRequestRepository
{
    private readonly AppDbContext _db;
    public DeletionRequestRepository(AppDbContext db) { _db = db; }     // הזרקה

    public void Add(EventDeletionRequest request) => _db.DeletionRequests.Add(request);     // רק מסמן הוספה

    public Task<EventDeletionRequest?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.DeletionRequests.Include(r => r.Event).ThenInclude(e => e.Client)   // במעקב, עם האירוע והלקוח (לתצוגה ולמחיקה)
            .FirstOrDefaultAsync(r => r.Id == id, ct);                          // בקשה אחת או null

    public Task<bool> HasPendingAsync(int eventId, CancellationToken ct) =>
        _db.DeletionRequests.AnyAsync(r => r.EventId == eventId && r.Status == DeletionRequestStatus.Pending, ct);  // בדיקה בלי לטעון שורות

    public Task<List<EventDeletionRequest>> GetListAsync(DeletionRequestStatus? status, CancellationToken ct) =>
        _db.DeletionRequests.AsNoTracking()                                     // קריאה בלבד
            .Include(r => r.Event).ThenInclude(e => e.Client)                   // לשדות התצוגה
            .Where(r => status == null || r.Status == status)                   // בלי סטטוס: כולן
            .OrderBy(r => r.CreatedAtUtc).ToListAsync(ct);                      // הוותיקות קודם
}
