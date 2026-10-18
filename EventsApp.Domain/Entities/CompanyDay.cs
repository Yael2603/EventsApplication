using System.ComponentModel.DataAnnotations;

namespace EventsApp.Domain.Entities;

public class CompanyDay
{
    public DateOnly Date { get; set; }
    public bool IsTaken { get; set; }

    [ConcurrencyCheck]
    public Guid Version { get; set; }
}
