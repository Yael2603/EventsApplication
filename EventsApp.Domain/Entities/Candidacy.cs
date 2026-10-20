namespace EventsApp.Domain.Entities;

public class Candidacy
{
    public int Id { get; set; }

    public int EventItemId { get; set; }
    public EventItem EventItem { get; set; } = null!;

    public int ProviderId { get; set; }
    public Provider Provider { get; set; } = null!;

    public string? Message { get; set; }
    public decimal ProposedPrice { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
