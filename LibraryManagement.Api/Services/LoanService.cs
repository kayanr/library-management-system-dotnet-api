using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Loans;
using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Services;

public class LoanService : ILoanService
{
    private readonly LibraryDbContext _context;
    private const int MaxActiveLoans = 3;

    public LoanService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<LoanResponse>> GetAllAsync()
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Select(l => MapToResponse(l))
            .ToListAsync();
    }

    public async Task<IEnumerable<LoanResponse>> GetByMemberAsync(int memberId)
    {
        return await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .Where(l => l.MemberId == memberId)
            .Select(l => MapToResponse(l))
            .ToListAsync();
    }

    public async Task<ServiceResult<LoanResponse>> BorrowBookAsync(CreateLoanRequest request)
    {
        var book = await _context.Books.FindAsync(request.BookId);
        if (book == null)
            return ServiceResult<LoanResponse>.Fail("Book not found.", ServiceErrorType.NotFound);

        var member = await _context.Members.FindAsync(request.MemberId);
        if (member == null)
            return ServiceResult<LoanResponse>.Fail("Member not found.", ServiceErrorType.NotFound);

        if (!book.Available)
            return ServiceResult<LoanResponse>.Fail("Book is not available.", ServiceErrorType.BusinessRule);

        if (!member.IsActive)
            return ServiceResult<LoanResponse>.Fail("Member is not active.", ServiceErrorType.BusinessRule);

        var activeLoans = await _context.Loans
            .CountAsync(l => l.MemberId == request.MemberId && !l.IsReturned);
        if (activeLoans >= MaxActiveLoans)
            return ServiceResult<LoanResponse>.Fail(
                $"Member already has {MaxActiveLoans} active loans.", ServiceErrorType.BusinessRule);

        var loan = new Loan
        {
            BookId = request.BookId,
            MemberId = request.MemberId,
            BorrowedDate = DateTime.UtcNow,
            IsReturned = false
        };

        book.Available = false;

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        await _context.Entry(loan).Reference(l => l.Book).LoadAsync();
        await _context.Entry(loan).Reference(l => l.Member).LoadAsync();

        return ServiceResult<LoanResponse>.Ok(MapToResponse(loan));
    }

    public async Task<ServiceResult<LoanResponse>> ReturnBookAsync(int loanId)
    {
        var loan = await _context.Loans
            .Include(l => l.Book)
            .Include(l => l.Member)
            .FirstOrDefaultAsync(l => l.Id == loanId);

        if (loan == null)
            return ServiceResult<LoanResponse>.Fail("Loan not found.", ServiceErrorType.NotFound);

        if (loan.IsReturned)
            return ServiceResult<LoanResponse>.Fail("Book has already been returned.", ServiceErrorType.BusinessRule);

        loan.IsReturned = true;
        loan.ReturnedDate = DateTime.UtcNow;
        loan.Book.Available = true;

        await _context.SaveChangesAsync();

        return ServiceResult<LoanResponse>.Ok(MapToResponse(loan));
    }

    private static LoanResponse MapToResponse(Loan loan) => new()
    {
        Id = loan.Id,
        BookId = loan.BookId,
        BookTitle = loan.Book.Title,
        MemberId = loan.MemberId,
        MemberName = $"{loan.Member.FirstName} {loan.Member.LastName}",
        BorrowedDate = loan.BorrowedDate,
        ReturnedDate = loan.ReturnedDate,
        IsReturned = loan.IsReturned
    };
}
