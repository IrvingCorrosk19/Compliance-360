namespace Compliance360.Application;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}

public interface ICurrentTenantContext
{
    Guid TenantId { get; }

    bool HasTenant { get; }
}

public interface ICurrentUserContext
{
    Guid UserId { get; }

    Guid TenantId { get; }

    string? Email { get; }

    bool IsAuthenticated { get; }
}

public interface IPasswordHasher
{
    string HashPassword(string password);

    PasswordVerificationResult Verify(string password, string passwordHash);
}

public enum PasswordVerificationResult
{
    Failed = 0,
    Success = 1
}

public interface IJwtTokenService
{
    JwtTokenResult CreateAccessToken(AuthenticatedUser user);
}

public sealed record AuthenticatedUser(
    Guid UserId,
    Guid TenantId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);

public sealed record JwtTokenResult(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc);

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "Compliance360";

    public string Audience { get; set; } = "Compliance360.Web";

    public string SigningKey { get; set; } = string.Empty;

    public int AccessTokenMinutes { get; set; } = 15;
}

public interface IRefreshTokenGenerator
{
    GeneratedRefreshToken Generate();
}

public sealed record GeneratedRefreshToken(
    string PlainTextToken,
    string TokenHash,
    DateTimeOffset ExpiresAtUtc);

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshTokens";

    public int LifetimeDays { get; set; } = 30;
}

public interface IFileStorageService
{
    Task<StoredFileDescriptor> SaveAsync(FileStorageRequest request, CancellationToken cancellationToken = default);
}

public sealed record FileStorageRequest(
    Guid TenantId,
    string FileName,
    string ContentType,
    Stream Content,
    string OwnerEntityName,
    Guid OwnerEntityId);

public sealed record StoredFileDescriptor(
    string StorageProvider,
    string ContainerName,
    string ObjectKey,
    long SizeBytes,
    string Sha256Hash);

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";

    public string ContainerName { get; set; } = "compliance360-documents";

    public string RootPath { get; set; } = "storage";
}
