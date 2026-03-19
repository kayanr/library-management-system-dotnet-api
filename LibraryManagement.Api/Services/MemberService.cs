using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Members;
using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagement.Api.Services;

public class MemberService : IMemberService
{
    private readonly LibraryDbContext _context;

    public MemberService(LibraryDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MemberResponse>> GetAllAsync()
    {
        return await _context.Members
            .Select(m => MapToResponse(m, 0))
            .ToListAsync();
    }

    public async Task<ServiceResult<MemberResponse>> GetByIdAsync(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
            return ServiceResult<MemberResponse>.Fail("Member not found.", ServiceErrorType.NotFound);

        return ServiceResult<MemberResponse>.Ok(MapToResponse(member, 0));
    }

    public async Task<ServiceResult<MemberResponse>> CreateAsync(CreateMemberRequest request)
    {
        var emailExists = await _context.Members.AnyAsync(m => m.Email == request.Email);
        if (emailExists)
            return ServiceResult<MemberResponse>.Fail("Email is already registered.", ServiceErrorType.Conflict);

        var member = new Member
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            MembershipDate = DateTime.UtcNow,
            IsActive = true
        };

        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        return ServiceResult<MemberResponse>.Ok(MapToResponse(member, 0));
    }

    public async Task<ServiceResult<MemberResponse>> UpdateAsync(int id, UpdateMemberRequest request)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
            return ServiceResult<MemberResponse>.Fail("Member not found.", ServiceErrorType.NotFound);

        var emailTaken = await _context.Members
            .AnyAsync(m => m.Email == request.Email && m.Id != id);
        if (emailTaken)
            return ServiceResult<MemberResponse>.Fail("Email is already registered.", ServiceErrorType.Conflict);

        member.FirstName = request.FirstName;
        member.LastName = request.LastName;
        member.Email = request.Email;
        member.Phone = request.Phone;
        member.IsActive = request.IsActive;

        await _context.SaveChangesAsync();

        return ServiceResult<MemberResponse>.Ok(MapToResponse(member, 0));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var member = await _context.Members.FindAsync(id);
        if (member == null)
            return ServiceResult<bool>.Fail("Member not found.", ServiceErrorType.NotFound);

        _context.Members.Remove(member);
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    private static MemberResponse MapToResponse(Member member, int activeLoanCount) => new()
    {
        Id = member.Id,
        FirstName = member.FirstName,
        LastName = member.LastName,
        Email = member.Email,
        Phone = member.Phone,
        MembershipDate = member.MembershipDate,
        IsActive = member.IsActive,
        ActiveLoanCount = activeLoanCount
    };
}
