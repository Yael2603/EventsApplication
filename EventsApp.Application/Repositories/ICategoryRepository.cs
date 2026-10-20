using EventsApp.Domain.Entities;

namespace EventsApp.Application.Repositories;

public interface ICategoryRepository               // גישה לקטגוריות
{
    Task<List<Category>> GetAllAsync(CancellationToken cancellationToken);              // קיימת: כל הקטגוריות
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken);                // קיימת: האם קטגוריה קיימת
    Task<Category?> GetByIdAsync(int id, CancellationToken ct);                         // חדש: קטגוריה במעקב (לעדכון/מחיקה)
    Task<bool> NameExistsAsync(string name, int? excludeId, CancellationToken ct);      // חדש: האם שם תפוס (מתעלם מקטגוריה מסוימת, לשינוי שם)
    Task<bool> IsInUseAsync(int id, CancellationToken ct);                              // חדש: האם יש ספקים או סעיפים בקטגוריה
    void Add(Category category);                                                        // חדש: מסמן הוספה
    void Remove(Category category);                                                     // חדש: מסמן מחיקה
}
