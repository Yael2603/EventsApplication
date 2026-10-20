using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.Categories;

public class SaveCategoryDto                        // גוף הבקשה גם להוספה וגם לשינוי שם
{
    [Required, MaxLength(100)]                      // חובה (גם לא רווחים בלבד), עד 100 תווים
    public string Name { get; set; } = string.Empty;    // שם הקטגוריה
}
