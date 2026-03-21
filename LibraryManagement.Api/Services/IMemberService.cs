using LibraryManagement.Api.DTOs.Members;

namespace LibraryManagement.Api.Services;

public interface IMemberService
{
    Task<IEnumerable<MemberResponse>> GetAllAsync();
    Task<ServiceResult<MemberResponse>> GetByIdAsync(int id);
    Task<ServiceResult<MemberResponse>> CreateAsync(CreateMemberRequest request);
    Task<ServiceResult<MemberResponse>> UpdateAsync(int id, UpdateMemberRequest request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
