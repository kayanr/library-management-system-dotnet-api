using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTOs.Members;

public class UpdateMemberRequest
{
    [Required]
    [StringLength(100)]
    public required string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public required string LastName { get; set; }

    [Required]
    [EmailAddress]
    public required string Email { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    public bool IsActive { get; set; }
}
