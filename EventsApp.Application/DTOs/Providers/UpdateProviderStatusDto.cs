using System.ComponentModel.DataAnnotations;
using EventsApp.Domain.Enums;

namespace EventsApp.Application.DTOs.Providers;

public class UpdateProviderStatusDto                // גוף הבקשה לאישור או דחייה
{
    [EnumDataType(typeof(ProviderStatus))]          // חייב להיות ערך קיים ב-Enum
    public ProviderStatus Status { get; set; }      // הסטטוס החדש (Pending נדחה בשירות)
}
