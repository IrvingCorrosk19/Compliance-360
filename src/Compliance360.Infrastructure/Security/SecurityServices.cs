using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Compliance360.Application;
using Compliance360.Application.Identity;
using Compliance360.Domain.Identity;
using Compliance360.Shared;
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

public sealed class MfaChallengeTokenService : IMfaChallengeTokenService
{
    private const char TokenDelimiter = '.';
    private const char PayloadDelimiter = '|';
    private readonly JwtOptions _jwtOptions;
    private readonly IClock _clock;
    private readonly MfaChallengeOptions _options;

    public MfaChallengeTokenService(IOptions<JwtOptions> jwtOptions, IOptions<MfaChallengeOptions> options, IClock clock)
    {
        _jwtOptions = jwtOptions.Value;
        _options = options.Value;
        _clock = clock;
    }

    public string Create(MfaChallengePrincipal principal)
    {
        var expiresAt = principal.ExpiresAtUtc <= _clock.UtcNow
            ? _clock.UtcNow.AddMinutes(_options.LifetimeMinutes)
            : principal.ExpiresAtUtc;
        var payload = string.Join(
            PayloadDelimiter,
            principal.TenantId,
            principal.UserId,
            (int)principal.Method,
            expiresAt.ToUnixTimeSeconds());
        var payloadBytes = Encoding.UTF8.GetBytes(payload);
        var payloadSegment = Base64UrlEncode(payloadBytes);
        var signatureSegment = Base64UrlEncode(Sign(payloadSegment));
        return $"{payloadSegment}{TokenDelimiter}{signatureSegment}";
    }

    public Result<MfaChallengePrincipal> Validate(string challengeToken)
    {
        if (string.IsNullOrWhiteSpace(challengeToken))
        {
            return Result<MfaChallengePrincipal>.Failure("Invalid MFA challenge.");
        }

        var segments = challengeToken.Split(TokenDelimiter);
        if (segments.Length != 2)
        {
            return Result<MfaChallengePrincipal>.Failure("Invalid MFA challenge.");
        }

        string[] payload;
        try
        {
            var expectedSignature = Sign(segments[0]);
            var actualSignature = Base64UrlDecode(segments[1]);
            if (!CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature))
            {
                return Result<MfaChallengePrincipal>.Failure("Invalid MFA challenge.");
            }

            payload = Encoding.UTF8.GetString(Base64UrlDecode(segments[0])).Split(PayloadDelimiter);
        }
        catch (FormatException)
        {
            return Result<MfaChallengePrincipal>.Failure("Invalid MFA challenge.");
        }

        if (payload.Length != 4
            || !Guid.TryParse(payload[0], out var tenantId)
            || !Guid.TryParse(payload[1], out var userId)
            || !int.TryParse(payload[2], out var methodValue)
            || !Enum.IsDefined(typeof(MfaMethod), methodValue)
            || !long.TryParse(payload[3], out var expiresUnix))
        {
            return Result<MfaChallengePrincipal>.Failure("Invalid MFA challenge.");
        }

        var expiresAt = DateTimeOffset.FromUnixTimeSeconds(expiresUnix);
        if (expiresAt <= _clock.UtcNow)
        {
            return Result<MfaChallengePrincipal>.Failure("MFA challenge expired.");
        }

        return Result<MfaChallengePrincipal>.Success(new MfaChallengePrincipal(tenantId, userId, (MfaMethod)methodValue, expiresAt));
    }

    private byte[] Sign(string payloadSegment)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_jwtOptions.SigningKey);
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadSegment));
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var padded = value.Replace('-', '+').Replace('_', '/');
        padded = padded.PadRight(padded.Length + ((4 - padded.Length % 4) % 4), '=');
        return Convert.FromBase64String(padded);
    }
}
