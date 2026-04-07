using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using CourseManagementApi.Data;
using CourseManagementApi.DTOs.Auth;
using CourseManagementApi.DTOs.User;
using CourseManagementApi.Entities;
using CourseManagementApi.Helpers;
using CourseManagementApi.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CourseManagementApi.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly JwtSettings _jwt;

    public AuthService(ApplicationDbContext context, IOptions<JwtSettings> jwtOptions)
    {
        _context = context;
        _jwt = jwtOptions.Value;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        return await BuildAuthResponseAsync(user, cancellationToken);
    }

    public async Task<AuthResponseDto?> RefreshAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var hashInput = request.RefreshToken.Trim();
        var existing = await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == hashInput, cancellationToken);

        if (existing == null || existing.IsRevoked || existing.ExpiresAt <= DateTime.UtcNow)
            return null;

        existing.IsRevoked = true;
        await _context.SaveChangesAsync(cancellationToken);

        return await BuildAuthResponseAsync(existing.User, cancellationToken);
    }

    private async Task<AuthResponseDto> BuildAuthResponseAsync(User user, CancellationToken cancellationToken)
    {
        var accessExpires = DateTime.UtcNow.AddMinutes(_jwt.AccessTokenExpirationMinutes);
        var accessToken = CreateAccessToken(user, accessExpires);

        var refreshPlain = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshPlain,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenExpirationDays),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.RefreshTokens.Add(refreshEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshPlain,
            AccessTokenExpiresAt = accessExpires,
            User = new UserReadDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role.ToString()
            }
        };
    }

    private string CreateAccessToken(User user, DateTime expiresUtc)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresUtc,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
