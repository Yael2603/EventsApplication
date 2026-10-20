using AutoMapper;
using EventsApp.Application.Abstractions;
using EventsApp.Application.DTOs.Categories;
using EventsApp.Application.Exceptions;
using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;

namespace EventsApp.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categories;   // קטגוריות
    private readonly IUnitOfWork _uow;                  // חדש: שמירה
    private readonly IMapper _mapper;                   // המרה

    public CategoryService(ICategoryRepository categories, IUnitOfWork uow, IMapper mapper)    // הזרקה
    {
        _categories = categories;
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<List<CategoryDto>> GetAllAsync(CancellationToken ct)      // קיימת, ללא שינוי
    {
        var items = await _categories.GetAllAsync(ct);
        return _mapper.Map<List<CategoryDto>>(items);
    }

    public async Task<CategoryDto> CreateAsync(SaveCategoryDto dto, CancellationToken ct)
    {
        var name = dto.Name.Trim();                                     // מסיר רווחים בקצוות
        if (await _categories.NameExistsAsync(name, null, ct))          // השם תפוס (בלי להתחשב באותיות)
            throw new ConflictException("קטגוריה בשם זה כבר קיימת");    // 409

        var category = new Category { Name = name };                    // ישות חדשה (שדה אחד, בלי צורך במיפוי)
        _categories.Add(category);                                      // מסמן הוספה
        await _uow.SaveChangesAsync(ct);                                // שומר; האינדקס הייחודי הוא רשת ביטחון ← 409
        return _mapper.Map<CategoryDto>(category);                      // מחזיר עם ה-Id החדש
    }

    public async Task<CategoryDto> UpdateAsync(int id, SaveCategoryDto dto, CancellationToken ct)
    {
        var category = await _categories.GetByIdAsync(id, ct)           // מחפש (במעקב)
                       ?? throw new NotFoundException("הקטגוריה לא נמצאה");     // 404

        var name = dto.Name.Trim();                                     // מסיר רווחים
        if (await _categories.NameExistsAsync(name, id, ct))            // תפוס על ידי קטגוריה אחרת
            throw new ConflictException("קטגוריה בשם זה כבר קיימת");    // 409

        category.Name = name;                                           // מעדכן; EF מזהה את השינוי
        await _uow.SaveChangesAsync(ct);                                // שומר
        return _mapper.Map<CategoryDto>(category);                      // מחזיר את המעודכן
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        var category = await _categories.GetByIdAsync(id, ct)           // מחפש (במעקב)
                       ?? throw new NotFoundException("הקטגוריה לא נמצאה");     // 404

        if (await _categories.IsInUseAsync(id, ct))                     // יש ספקים או סעיפים
            throw new ConflictException("לא ניתן למחוק קטגוריה שיש בה ספקים או סעיפי אירועים");   // 409

        _categories.Remove(category);                                   // מסמן מחיקה
        await _uow.SaveChangesAsync(ct);                                // שומר
    }
}
