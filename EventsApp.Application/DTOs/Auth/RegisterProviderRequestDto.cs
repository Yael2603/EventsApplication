using System.ComponentModel.DataAnnotations;

namespace EventsApp.Application.DTOs.Auth;

public class RegisterProviderRequestDto
{
    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string BusinessName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    [Required, MaxLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string City { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int CategoryId { get; set; }

    [Range(0, 1000000)]
    public decimal StartingPrice { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "יש לאשר את תנאי השימוש")]
    public bool AcceptTerms { get; set; }
}
