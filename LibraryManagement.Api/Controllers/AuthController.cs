using LibraryManagement.Api.DTOs.Auth;
using LibraryManagement.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST: api/auth/register
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
            return MapServiceError(result.ErrorType, result.ErrorMessage!);

        return CreatedAtAction(nameof(Register), result.Value);
    }

    // POST: api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

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
