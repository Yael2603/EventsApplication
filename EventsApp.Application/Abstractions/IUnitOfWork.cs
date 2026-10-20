namespace EventsApp.Application.Abstractions;       // ליד שאר ההפשטות (IPasswordHasher, ITokenGenerator)

public interface IUnitOfWork                        // חוזה: שמירת כל השינויים בבת אחת
{
    Task SaveChangesAsync(CancellationToken ct);    // זורק ConcurrencyException או ConflictException במקרה התנגשות
}
