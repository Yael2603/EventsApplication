using EventsApp.Application.DTOs.Categories;

namespace EventsApp.Application.Services;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetAllAsync(CancellationToken ct);                          // קיימת
    Task<CategoryDto> CreateAsync(SaveCategoryDto dto, CancellationToken ct);           // הוספה
    Task<CategoryDto> UpdateAsync(int id, SaveCategoryDto dto, CancellationToken ct);   // שינוי שם
    Task DeleteAsync(int id, CancellationToken ct);                                     // מחיקה
}
