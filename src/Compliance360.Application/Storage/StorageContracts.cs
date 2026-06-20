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
}

public interface IStorageRepository
{
    Task AddAsync(StoredFile storedFile, CancellationToken cancellationToken = default);

    Task<StoredFile?> GetByIdAsync(Guid tenantId, Guid storedFileId, CancellationToken cancellationToken = default);

    Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
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
