namespace LibraryManagement.Api.DTOs.Members;

public class MemberResponse
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime MembershipDate { get; set; }
    public bool IsActive { get; set; }
    public int ActiveLoanCount { get; set; }
}
