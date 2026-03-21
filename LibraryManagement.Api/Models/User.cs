using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.Models;

public class User
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public required string Email { get; set; }

    [Required]
    public required string PasswordHash { get; set; }

    [Required]
    [StringLength(50)]
    public required string Role { get; set; }

    public DateTime CreatedAt { get; set; }
}
