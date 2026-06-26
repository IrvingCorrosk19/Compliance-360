using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Storage;
using Compliance360.Shared;

namespace Compliance360.Application.Storage;

public sealed class StorageFoundationService : IStorageFoundationService
{
    private readonly IStorageRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorageService _fileStorageService;
    private readonly IStorageProviderFactory? _storageProviderFactory;
    private readonly IClock _clock;

    public StorageFoundationService(
        IStorageRepository repository,
        IApplicationDbContext dbContext,
        IFileStorageService fileStorageService,
        IClock clock)
    {
        _repository = repository;
        _dbContext = dbContext;
        _fileStorageService = fileStorageService;
        _clock = clock;
    }

    public StorageFoundationService(
        IStorageRepository repository,
        IApplicationDbContext dbContext,
        IFileStorageService fileStorageService,
        IStorageProviderFactory storageProviderFactory,
        IClock clock)
        : this(repository, dbContext, fileStorageService, clock)
    {
        _storageProviderFactory = storageProviderFactory;
    }

    public async Task<Result<StoredFileSummary>> UploadAsync(UploadFileCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.AgainstEmpty(command.TenantId, nameof(command.TenantId));
            Guard.AgainstEmpty(command.RequestedByUserId, nameof(command.RequestedByUserId));
            Guard.AgainstEmpty(command.OwnerEntityId, nameof(command.OwnerEntityId));
            Guard.AgainstNullOrWhiteSpace(command.OwnerEntityName, nameof(command.OwnerEntityName), 160);
            Guard.AgainstNullOrWhiteSpace(command.FileName, nameof(command.FileName), 260);
            Guard.AgainstNullOrWhiteSpace(command.ContentType, nameof(command.ContentType), 120);

            var storageRequest = new FileStorageRequest(command.TenantId, command.FileName, command.ContentType, command.Content, command.OwnerEntityName, command.OwnerEntityId);
            var descriptor = _storageProviderFactory is null
                ? await _fileStorageService.SaveAsync(storageRequest, cancellationToken)
                : await SaveWithConfiguredProviderAsync(storageRequest, command.RequestedByUserId, cancellationToken);

            var storedFile = new StoredFile(
                command.TenantId,
                descriptor.StorageProvider,
                descriptor.ContainerName,
                descriptor.ObjectKey,
                command.FileName,
                command.ContentType,
                descriptor.SizeBytes,
                descriptor.Sha256Hash,
                command.OwnerEntityName,
                command.OwnerEntityId);

            if (command.VersionEntityId.HasValue)
            {
                storedFile.LinkVersion(command.VersionEntityId.Value);
            }

            await _repository.AddAsync(storedFile, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, storedFile.Id, AuditAction.FileUploaded, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<StoredFileSummary>.Success(ToSummary(storedFile));
        }
        catch (DomainException exception)
        {
            return Result<StoredFileSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<StoredFileSummary>> GetAsync(GetStoredFileQuery query, CancellationToken cancellationToken = default)
    {
        var storedFile = await _repository.GetByIdAsync(query.TenantId, query.StoredFileId, cancellationToken);
        return storedFile is null
            ? Result<StoredFileSummary>.Failure("Stored file not found.")
            : Result<StoredFileSummary>.Success(ToSummary(storedFile));
    }

    public async Task<Result<StoredFileDownloadDescriptor>> RegisterDownloadAsync(RegisterFileDownloadCommand command, CancellationToken cancellationToken = default)
    {
        var storedFile = await _repository.GetByIdAsync(command.TenantId, command.StoredFileId, cancellationToken);
        if (storedFile is null || storedFile.Status is StoredFileStatus.Deleted or StoredFileStatus.Quarantined)
        {
            return Result<StoredFileDownloadDescriptor>.Failure("Stored file is not available for download.");
        }

        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, storedFile.Id, AuditAction.FileDownloaded, true, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<StoredFileDownloadDescriptor>.Success(new StoredFileDownloadDescriptor(
            storedFile.Id,
            storedFile.TenantId,
            storedFile.StorageProvider,
            storedFile.ContainerName,
            storedFile.ObjectKey,
            storedFile.OriginalFileName,
            storedFile.ContentType,
            storedFile.SizeBytes,
            storedFile.Sha256Hash));
    }

    public async Task<Result> MarkAvailableAsync(ChangeStoredFileStatusCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeStatusAsync(command, file => file.MarkAvailable(), AuditAction.Updated, cancellationToken);
    }

    public async Task<Result> QuarantineAsync(ChangeStoredFileStatusCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeStatusAsync(command, file => file.Quarantine(), AuditAction.SecurityEvent, cancellationToken);
    }

    public async Task<Result> DeleteAsync(ChangeStoredFileStatusCommand command, CancellationToken cancellationToken = default)
    {
        return await ChangeStatusAsync(command, file => file.Delete(), AuditAction.Deleted, cancellationToken);
    }

    public async Task<Result<StorageProviderConfigurationSummary>> CreateProviderAsync(CreateStorageProviderConfigurationCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var configuration = new StorageProviderConfiguration(command.TenantId, command.Provider, command.Name, command.ContainerName, command.Priority, command.IsDefault, command.IsEnabled, command.SettingsJson);
            await _repository.AddProviderConfigurationAsync(configuration, cancellationToken);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, configuration.Id, AuditAction.StorageProviderChanged, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result<StorageProviderConfigurationSummary>.Success(ToProviderSummary(configuration));
        }
        catch (DomainException exception)
        {
            return Result<StorageProviderConfigurationSummary>.Failure(exception.Message);
        }
    }

    public async Task<Result<StorageProviderConfigurationSummary>> UpdateProviderAsync(UpdateStorageProviderConfigurationCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = await _repository.GetProviderConfigurationAsync(command.TenantId, command.ProviderConfigurationId, cancellationToken);
        if (configuration is null)
        {
            return Result<StorageProviderConfigurationSummary>.Failure("Storage provider configuration not found.");
        }

        configuration.Update(command.Name, command.ContainerName, command.Priority, command.IsDefault, command.IsEnabled, command.SettingsJson);
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, configuration.Id, AuditAction.StorageProviderChanged, true, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<StorageProviderConfigurationSummary>.Success(ToProviderSummary(configuration));
    }

    public async Task<Result> DisableProviderAsync(ChangeStorageProviderCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = await _repository.GetProviderConfigurationAsync(command.TenantId, command.ProviderConfigurationId, cancellationToken);
        if (configuration is null)
        {
            return Result.Failure("Storage provider configuration not found.");
        }

        configuration.Disable();
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, configuration.Id, AuditAction.StorageProviderChanged, true, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<Result<StorageProviderConfigurationSummary>> SetActiveProviderAsync(ChangeStorageProviderCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = await _repository.GetProviderConfigurationAsync(command.TenantId, command.ProviderConfigurationId, cancellationToken);
        if (configuration is null)
        {
            return Result<StorageProviderConfigurationSummary>.Failure("Storage provider configuration not found.");
        }

        configuration.MarkDefault();
        await AppendAuditAsync(command.TenantId, command.RequestedByUserId, configuration.Id, AuditAction.StorageProviderChanged, true, null, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<StorageProviderConfigurationSummary>.Success(ToProviderSummary(configuration));
    }

    public async Task<Result<StorageProviderHealthSummary>> TestProviderAsync(ChangeStorageProviderCommand command, CancellationToken cancellationToken = default)
    {
        var configuration = await _repository.GetProviderConfigurationAsync(command.TenantId, command.ProviderConfigurationId, cancellationToken);
        if (configuration is null || _storageProviderFactory is null)
        {
            return Result<StorageProviderHealthSummary>.Failure("Storage provider configuration not found.");
        }

        var result = await _storageProviderFactory.GetProvider(configuration.Provider).CheckHealthAsync(configuration, cancellationToken);
        configuration.RecordHealth(result.Healthy, result.Message, _clock.UtcNow);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<StorageProviderHealthSummary>.Success(result);
    }

    public async Task<Result<IReadOnlyCollection<StorageProviderConfigurationSummary>>> ListProvidersAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var providers = await _repository.ListProviderConfigurationsAsync(tenantId, cancellationToken);
        return Result<IReadOnlyCollection<StorageProviderConfigurationSummary>>.Success(providers.Select(ToProviderSummary).ToArray());
    }

    private async Task<Result> ChangeStatusAsync(
        ChangeStoredFileStatusCommand command,
        Action<StoredFile> changeStatus,
        AuditAction action,
        CancellationToken cancellationToken)
    {
        try
        {
            var storedFile = await _repository.GetByIdAsync(command.TenantId, command.StoredFileId, cancellationToken);
            if (storedFile is null)
            {
                return Result.Failure("Stored file not found.");
            }

            changeStatus(storedFile);
            await AppendAuditAsync(command.TenantId, command.RequestedByUserId, storedFile.Id, action, true, null, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (DomainException exception)
        {
            return Result.Failure(exception.Message);
        }
    }

    private async Task AppendAuditAsync(Guid tenantId, Guid userId, Guid storedFileId, AuditAction action, bool success, string? error, CancellationToken cancellationToken)
    {
        var auditLog = AuditLog.FromEvent(
            new AuditEvent(
                nameof(StoredFile),
                storedFileId,
                action,
                AuditLog.InferCategory(action),
                new AuditContext(tenantId, userId, null, null, null, null, null, null, null),
                new AuditSnapshot(null, null),
                new AuditMetadata("{\"source\":\"storage\"}"),
                success,
                error),
            _clock.UtcNow);

        await _repository.AddAuditLogAsync(auditLog, cancellationToken);
    }

    private async Task<StoredFileDescriptor> SaveWithConfiguredProviderAsync(FileStorageRequest request, Guid userId, CancellationToken cancellationToken)
    {
        var configurations = (await _repository.ListProviderConfigurationsAsync(request.TenantId, cancellationToken))
            .Where(configuration => configuration.IsEnabled)
            .OrderByDescending(configuration => configuration.IsDefault)
            .ThenBy(configuration => configuration.Priority)
            .ToArray();
        foreach (var configuration in configurations)
        {
            try
            {
                return await _storageProviderFactory!.GetProvider(configuration.Provider).SaveAsync(request, configuration, cancellationToken);
            }
            catch (Exception exception)
            {
                await AppendAuditAsync(request.TenantId, userId, configuration.Id, AuditAction.StorageProviderFailover, false, exception.Message, cancellationToken);
            }
        }

        return await _fileStorageService.SaveAsync(request, cancellationToken);
    }

    private static StoredFileSummary ToSummary(StoredFile storedFile)
    {
        return new StoredFileSummary(
            storedFile.Id,
            storedFile.TenantId,
            storedFile.StorageProvider,
            storedFile.ContainerName,
            storedFile.ObjectKey,
            storedFile.OriginalFileName,
            storedFile.ContentType,
            storedFile.SizeBytes,
            storedFile.Sha256Hash,
            storedFile.OwnerEntityName,
            storedFile.OwnerEntityId,
            storedFile.VersionEntityId,
            storedFile.Status);
    }

    private static StorageProviderConfigurationSummary ToProviderSummary(StorageProviderConfiguration configuration)
    {
        return new StorageProviderConfigurationSummary(configuration.Id, configuration.TenantId, configuration.Provider, configuration.Name, configuration.ContainerName, configuration.Priority, configuration.IsDefault, configuration.IsEnabled, configuration.LastHealthStatus, configuration.LastHealthMessage);
    }
}
