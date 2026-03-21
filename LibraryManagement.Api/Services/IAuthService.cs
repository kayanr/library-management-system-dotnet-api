using LibraryManagement.Api.DTOs.Auth;

namespace LibraryManagement.Api.Services;

public interface IAuthService
{
    Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request);
    Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request);
}
