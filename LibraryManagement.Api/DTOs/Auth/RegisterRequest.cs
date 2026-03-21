using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTOs.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [Required]
    [MinLength(6)]
    public required string Password { get; set; }

    [Required]
    public required string Role { get; set; }
}
