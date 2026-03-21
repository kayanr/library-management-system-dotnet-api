using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Api.DTOs.Loans;

public class CreateLoanRequest
{
    [Required]
    public int BookId { get; set; }

    [Required]
    public int MemberId { get; set; }
}
