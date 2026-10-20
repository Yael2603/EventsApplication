using EventsApp.Application.DTOs.Categories;
using EventsApp.Application.Services;
using EventsApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventsApp.API.Controllers;

[ApiController]
[Route("api/categories")]                           // /api/categories
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories)    // הזרקה
    {
        _categories = categories;
    }

    [HttpGet]                                       // GET /api/categories (קיימת)
    [AllowAnonymous]                                // פתוחה לכולם (גם לטופס הרשמה)
    public async Task<ActionResult<List<CategoryDto>>> GetAll(CancellationToken ct)
        => Ok(await _categories.GetAllAsync(ct));

    [HttpPost]                                      // POST /api/categories
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<ActionResult<CategoryDto>> Create([FromBody] SaveCategoryDto dto, CancellationToken ct)
        => StatusCode(StatusCodes.Status201Created, await _categories.CreateAsync(dto, ct));    // 201

    [HttpPut("{id:int}")]                           // PUT /api/categories/4
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<ActionResult<CategoryDto>> Update([FromRoute] int id, [FromBody] SaveCategoryDto dto, CancellationToken ct)
        => Ok(await _categories.UpdateAsync(id, dto, ct));      // 200

    [HttpDelete("{id:int}")]                        // DELETE /api/categories/4
    [Authorize(Roles = nameof(UserRole.Admin))]     // אדמין בלבד
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken ct)
    {
        await _categories.DeleteAsync(id, ct);      // מוחק (או זורק 404/409)
        return NoContent();                         // 204: הצליח, בלי גוף תשובה
    }
}
