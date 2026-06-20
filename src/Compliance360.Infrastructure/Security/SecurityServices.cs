using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Compliance360.Application;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Compliance360.Infrastructure.Security;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 210_000;
    private const char SegmentDelimiter = '.';

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, KeySize);

        return string.Join(
            SegmentDelimiter,
            "PBKDF2-SHA256",
            Iterations,
            Convert.ToBase64String(salt),
            Convert.ToBase64String(key));
    }

    public PasswordVerificationResult Verify(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return PasswordVerificationResult.Failed;
        }

        var segments = passwordHash.Split(SegmentDelimiter);
        if (segments.Length != 4 || segments[0] != "PBKDF2-SHA256")
        {
            return PasswordVerificationResult.Failed;
        }

        if (!int.TryParse(segments[1], out var iterations))
        {
            return PasswordVerificationResult.Failed;
        }

        var salt = Convert.FromBase64String(segments[2]);
        var expectedKey = Convert.FromBase64String(segments[3]);
        var actualKey = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expectedKey.Length);

        return CryptographicOperations.FixedTimeEquals(actualKey, expectedKey)
            ? PasswordVerificationResult.Success
            : PasswordVerificationResult.Failed;
    }
}

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;
    private readonly IClock _clock;

    public JwtTokenService(IOptions<JwtOptions> options, IClock clock)
    {
        _options = options.Value;
        _clock = clock;
    }

    public JwtTokenResult CreateAccessToken(AuthenticatedUser user)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.SigningKey);

        var now = _clock.UtcNow;
        var expiresAt = now.AddMinutes(_options.AccessTokenMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("tenant_id", user.TenantId.ToString()),
            new("name", user.FullName)
        };

        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(user.Permissions.Select(permission => new Claim("permission", permission)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return new JwtTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private readonly IClock _clock;
    private readonly RefreshTokenOptions _options;

    public RefreshTokenGenerator(IClock clock, IOptions<RefreshTokenOptions> options)
    {
        _clock = clock;
        _options = options.Value;
    }

    public GeneratedRefreshToken Generate()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var plainTextToken = Convert.ToBase64String(tokenBytes);
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(plainTextToken)));

        return new GeneratedRefreshToken(
            plainTextToken,
            tokenHash,
            _clock.UtcNow.AddDays(_options.LifetimeDays));
    }
}
