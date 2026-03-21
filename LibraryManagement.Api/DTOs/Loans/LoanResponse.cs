namespace LibraryManagement.Api.DTOs.Loans;

public class LoanResponse
{
    public int Id { get; set; }
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateTime BorrowedDate { get; set; }
    public DateTime? ReturnedDate { get; set; }
    public bool IsReturned { get; set; }
}
