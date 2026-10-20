using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db)      // הזרקה
    {
        _db = db;
    }

    public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken) =>
        _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);  // קיימת, ללא שינוי

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        _db.Categories.AnyAsync(c => c.Id == id, cancellationToken);                        // קיימת, ללא שינוי

    public Task<Category?> GetByIdAsync(int id, CancellationToken ct) =>
        _db.Categories.FirstOrDefaultAsync(c => c.Id == id, ct);        // במעקב, כדי ששינוי שם או מחיקה יישמרו

    public Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct) =>
        _db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower()         // השוואה בלי הבדל אותיות
                                     && (excludeId == null || c.Id != excludeId), ct);  // בשינוי שם: לא נחשב התנגשות עם עצמה

    public async Task<bool> IsInUseAsync(int id, CancellationToken ct) =>
        await _db.Providers.AnyAsync(p => p.CategoryId == id, ct)               // יש ספק בקטגוריה
        || await _db.EventItems.AnyAsync(i => i.CategoryId == id, ct);          // או סעיף של אירוע

    public void Add(Category category) => _db.Categories.Add(category);         // רק מסמן הוספה
    public void Remove(Category category) => _db.Categories.Remove(category);   // רק מסמן מחיקה
}
