using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _db;
    public NotificationRepository(AppDbContext db) { _db = db; }    // הזרקה

    public void Add(Notification notification) => _db.Notifications.Add(notification);      // רק מסמן הוספה

    public Task<List<Notification>> GetByUserAsync(int userId, CancellationToken ct) =>
        _db.Notifications.AsNoTracking()                                // קריאה בלבד
            .Where(n => n.UserId == userId)                             // רק של המשתמש
            .OrderByDescending(n => n.CreatedAtUtc).ToListAsync(ct);    // החדשות קודם

    public Task<Notification?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Notifications.FirstOrDefaultAsync(n => n.Id == id, ct);     // במעקב, כדי ש-IsRead יישמר
}
