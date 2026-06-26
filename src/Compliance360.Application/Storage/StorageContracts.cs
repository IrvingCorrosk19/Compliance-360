using Compliance360.Domain.Audit;
using Compliance360.Domain.Storage;
using Compliance360.Shared;

namespace Compliance360.Application.Storage;

public interface IStorageFoundationService
{
    Task<Result<StoredFileSummary>> UploadAsync(UploadFileCommand command, CancellationToken cancellationToken = default);

    Task<Result<StoredFileSummary>> GetAsync(GetStoredFileQuery query, CancellationToken cancellationToken = default);

    Task<Result<StoredFileDownloadDescriptor>> RegisterDownloadAsync(RegisterFileDownloadCommand command, CancellationToken cancellationToken = default);

    Task<Result> MarkAvailableAsync(ChangeStoredFileStatusCommand command, CancellationToken cancellationToken = default);

    Task<Result> QuarantineAsync(ChangeStoredFileStatusCommand command, CancellationToken cancellationToken = default);

    Task<Result> DeleteAsync(ChangeStoredFileStatusCommand command, CancellationToken cancellationToken = default);

    Task<Result<StorageProviderConfigurationSummary>> CreateProviderAsync(CreateStorageProviderConfigurationCommand command, CancellationToken cancellationToken = default);

    Task<Result<StorageProviderConfigurationSummary>> UpdateProviderAsync(UpdateStorageProviderConfigurationCommand command, CancellationToken cancellationToken = default);

    Task<Result> DisableProviderAsync(ChangeStorageProviderCommand command, CancellationToken cancellationToken = default);

    Task<Result<StorageProviderConfigurationSummary>> SetActiveProviderAsync(ChangeStorageProviderCommand command, CancellationToken cancellationToken = default);

    Task<Result<StorageProviderHealthSummary>> TestProviderAsync(ChangeStorageProviderCommand command, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyCollection<StorageProviderConfigurationSummary>>> ListProvidersAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IStorageRepository
{
    Task AddAsync(StoredFile storedFile, CancellationToken cancellationToken = default);

    Task<StoredFile?> GetByIdAsync(Guid tenantId, Guid storedFileId, CancellationToken cancellationToken = default);

    Task AddProviderConfigurationAsync(StorageProviderConfiguration configuration, CancellationToken cancellationToken = default);

    Task<StorageProviderConfiguration?> GetProviderConfigurationAsync(Guid tenantId, Guid providerConfigurationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<StorageProviderConfiguration>> ListProviderConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}

public interface IStorageProvider
{
    StorageProviderKind Provider { get; }

    Task<StoredFileDescriptor> SaveAsync(FileStorageRequest request, StorageProviderConfiguration configuration, CancellationToken cancellationToken = default);

    Task<StorageProviderHealthSummary> CheckHealthAsync(StorageProviderConfiguration configuration, CancellationToken cancellationToken = default);
}

public interface IStorageProviderFactory
{
    IStorageProvider GetProvider(StorageProviderKind provider);
}

public sealed record UploadFileCommand(
    Guid TenantId,
    Guid RequestedByUserId,
    string FileName,
    string ContentType,
    Stream Content,
    string OwnerEntityName,
    Guid OwnerEntityId,
    Guid? VersionEntityId);

public sealed record GetStoredFileQuery(Guid TenantId, Guid StoredFileId, Guid RequestedByUserId);

public sealed record RegisterFileDownloadCommand(Guid TenantId, Guid StoredFileId, Guid RequestedByUserId);

public sealed record ChangeStoredFileStatusCommand(Guid TenantId, Guid StoredFileId, Guid RequestedByUserId);

public sealed record CreateStorageProviderConfigurationCommand(Guid TenantId, Guid RequestedByUserId, StorageProviderKind Provider, string Name, string ContainerName, int Priority, bool IsDefault, bool IsEnabled, string SettingsJson);

public sealed record UpdateStorageProviderConfigurationCommand(Guid TenantId, Guid RequestedByUserId, Guid ProviderConfigurationId, string Name, string ContainerName, int Priority, bool IsDefault, bool IsEnabled, string SettingsJson);

public sealed record ChangeStorageProviderCommand(Guid TenantId, Guid RequestedByUserId, Guid ProviderConfigurationId);

public sealed record StoredFileSummary(
    Guid Id,
    Guid TenantId,
    string StorageProvider,
    string ContainerName,
    string ObjectKey,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string Sha256Hash,
    string OwnerEntityName,
    Guid OwnerEntityId,
    Guid? VersionEntityId,
    StoredFileStatus Status);

public sealed record StoredFileDownloadDescriptor(
    Guid Id,
    Guid TenantId,
    string StorageProvider,
    string ContainerName,
    string ObjectKey,
    string OriginalFileName,
    string ContentType,
    long SizeBytes,
    string Sha256Hash);

public sealed record StorageProviderConfigurationSummary(Guid Id, Guid TenantId, StorageProviderKind Provider, string Name, string ContainerName, int Priority, bool IsDefault, bool IsEnabled, bool LastHealthStatus, string? LastHealthMessage);

public sealed record StorageProviderHealthSummary(StorageProviderKind Provider, bool Healthy, string Message);
