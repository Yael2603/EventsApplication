using EventsApp.Domain.Enums;

namespace EventsApp.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Provider? Provider { get; set; }
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
