using EventsApp.Domain.Enums;

namespace EventsApp.Domain.Entities;

public class Event
{
    public int Id { get; set; }

    public int ClientId { get; set; }
    public User Client { get; set; } = null!;

    public DateOnly Date { get; set; }
    public CompanyDay Day { get; set; } = null!;

    public EventType Type { get; set; }
    public string Location { get; set; } = string.Empty;
    public int GuestCount { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<EventItem> Items { get; set; } = new List<EventItem>();
}
