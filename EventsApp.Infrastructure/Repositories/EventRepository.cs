using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly AppDbContext _db;
    public EventRepository(AppDbContext db) { _db = db; }       // הזרקה

    private IQueryable<Event> WithDetails() =>          // שאילתת בסיס משותפת, כדי לא לחזור על ה-Include
        _db.Events.AsNoTracking()                       // קריאה בלבד
            .Include(e => e.Client)                     // טוען את הלקוח (לשמו)
            .Include(e => e.Items).ThenInclude(i => i.Category)     // טוען סעיפים וקטגוריה של כל סעיף
            .Include(e => e.Items).ThenInclude(i => i.Provider);    // טוען גם את הספק של כל סעיף (אם נבחר)

    public void Add(Event ev) => _db.Events.Add(ev);    // רק מסמן להוספה; השמירה ב-UnitOfWork

    public void Remove(Event ev) => _db.Events.Remove(ev);      // רק מסמן מחיקה; הסעיפים והמועמדויות נמחקים ב-Cascade ב-DB

    public Task<Event?> GetByIdAsync(int id, CancellationToken ct) =>
        WithDetails().FirstOrDefaultAsync(e => e.Id == id, ct);     // אירוע אחד או null

    public Task<List<Event>> GetByClientAsync(int clientId, CancellationToken ct) =>
        WithDetails().Where(e => e.ClientId == clientId)            // רק של הלקוח
            .OrderBy(e => e.Date).ToListAsync(ct);                  // לפי תאריך

    public Task<List<Event>> GetAllAsync(CancellationToken ct) =>
        WithDetails().OrderBy(e => e.Date).ToListAsync(ct);         // כולם, לפי תאריך

    public Task<Event?> GetForUpdateAsync(int id, CancellationToken ct) =>
        _db.Events.Include(e => e.Items)                // במעקב (בלי AsNoTracking), עם הסעיפים לבדיקת קטגוריות
            .FirstOrDefaultAsync(e => e.Id == id, ct);  // אירוע אחד או null
}
