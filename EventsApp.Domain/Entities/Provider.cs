using EventsApp.Domain.Enums;

namespace EventsApp.Domain.Entities;

public class Provider
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public string BusinessName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public decimal StartingPrice { get; set; }
    public ProviderStatus Status { get; set; } = ProviderStatus.Pending;
    public DateTime AcceptedTermsAt { get; set; }

    public ICollection<Candidacy> Candidacies { get; set; } = new List<Candidacy>();
    public ICollection<EventItem> BookedItems { get; set; } = new List<EventItem>();
}
