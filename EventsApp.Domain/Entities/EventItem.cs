namespace EventsApp.Domain.Entities;

public class EventItem
{
    public int Id { get; set; }

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public decimal Budget { get; set; }
    public string? Details { get; set; }

    public int? ProviderId { get; set; }
    public Provider? Provider { get; set; }

    public ICollection<Candidacy> Candidacies { get; set; } = new List<Candidacy>();
}
