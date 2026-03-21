using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryManagement.Api.Data;
using LibraryManagement.Api.DTOs.Auth;
using LibraryManagement.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagement.Api.Services;

public class AuthService : IAuthService
{
    private readonly LibraryDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(LibraryDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request)
    {
        var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email);
        if (emailExists)
            return ServiceResult<AuthResponse>.Fail("Email is already registered.", ServiceErrorType.Conflict);

        var user = new User
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return ServiceResult<AuthResponse>.Ok(GenerateToken(user));
    }

    public async Task<ServiceResult<AuthResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ServiceResult<AuthResponse>.Fail("Invalid email or password.", ServiceErrorType.Validation);

        return ServiceResult<AuthResponse>.Ok(GenerateToken(user));
    }

    private AuthResponse GenerateToken(User user)
    {
        var jwtKey = _configuration["Jwt:Key"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;
        var expiryMinutes = int.Parse(_configuration["Jwt:ExpiryMinutes"]!);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: expiry,
            signingCredentials: credentials
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Email = user.Email,
            Role = user.Role,
            ExpiresAt = expiry
        };
    }
}
