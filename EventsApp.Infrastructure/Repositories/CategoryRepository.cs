using EventsApp.Application.Repositories;
using EventsApp.Domain.Entities;
using EventsApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EventsApp.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Category>> GetAllAsync(CancellationToken cancellationToken) =>
        _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken);

    public Task<bool> ExistsAsync(int id, CancellationToken cancellationToken) =>
        _db.Categories.AnyAsync(c => c.Id == id, cancellationToken);
}
