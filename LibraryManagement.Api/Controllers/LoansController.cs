using LibraryManagement.Api.DTOs.Loans;
using LibraryManagement.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LoansController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoansController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    // GET: api/loans
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var loans = await _loanService.GetAllAsync();
        return Ok(loans);
    }

    // GET: api/loans/member/1
    [HttpGet("member/{memberId}")]
    public async Task<IActionResult> GetByMember(int memberId)
    {
        var loans = await _loanService.GetByMemberAsync(memberId);
        return Ok(loans);
    }

    // POST: api/loans/borrow
    [HttpPost("borrow")]
    public async Task<IActionResult> Borrow(CreateLoanRequest request)
    {
        var result = await _loanService.BorrowBookAsync(request);

        if (!result.Success)
            return MapServiceError(result.ErrorType, result.ErrorMessage!);

        return CreatedAtAction(nameof(GetAll), new { id = result.Value!.Id }, result.Value);
    }

    // PUT: api/loans/return/1
    [HttpPut("return/{loanId}")]
    public async Task<IActionResult> Return(int loanId)
    {
        var result = await _loanService.ReturnBookAsync(loanId);

        if (!result.Success)
            return MapServiceError(result.ErrorType, result.ErrorMessage!);

        return Ok(result.Value);
    }

    private IActionResult MapServiceError(ServiceErrorType? errorType, string message) =>
        errorType switch
        {
            ServiceErrorType.NotFound => NotFound(new { message }),
            ServiceErrorType.Conflict => Conflict(new { message }),
            ServiceErrorType.BusinessRule => UnprocessableEntity(new { message }),
            ServiceErrorType.Validation => BadRequest(new { message }),
            _ => StatusCode(500, new { message })
        };
}
