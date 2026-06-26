using Compliance360.Domain.Common;

namespace Compliance360.Domain.Storage;

public enum StoredFileStatus
{
    PendingScan = 0,
    Available = 1,
    Quarantined = 2,
    Archived = 3,
    Deleted = 4
}

public enum StorageProviderKind
{
    Local = 0,
    AzureBlob = 1,
    AwsS3 = 2,
    MinIO = 3,
    GoogleCloudStorage = 4,
    Sftp = 5
}

public sealed class StoredFile : TenantEntity
{
    private StoredFile()
    {
        StorageProvider = string.Empty;
        ContainerName = string.Empty;
        ObjectKey = string.Empty;
        OriginalFileName = string.Empty;
        ContentType = string.Empty;
        Sha256Hash = string.Empty;
        OwnerEntityName = string.Empty;
    }

    public StoredFile(
        Guid tenantId,
        string storageProvider,
        string containerName,
        string objectKey,
        string originalFileName,
        string contentType,
        long sizeBytes,
        string sha256Hash,
        string ownerEntityName,
        Guid ownerEntityId)
        : base(tenantId)
    {
        StorageProvider = Guard.AgainstNullOrWhiteSpace(storageProvider, nameof(storageProvider), 80);
        ContainerName = Guard.AgainstNullOrWhiteSpace(containerName, nameof(containerName), 120);
        ObjectKey = Guard.AgainstNullOrWhiteSpace(objectKey, nameof(objectKey), 500);
        OriginalFileName = Guard.AgainstNullOrWhiteSpace(originalFileName, nameof(originalFileName), 260);
        ContentType = Guard.AgainstNullOrWhiteSpace(contentType, nameof(contentType), 120);
        SizeBytes = sizeBytes > 0 ? sizeBytes : throw new DomainException("File size must be greater than zero.");
        Sha256Hash = Guard.AgainstNullOrWhiteSpace(sha256Hash, nameof(sha256Hash), 128);
        OwnerEntityName = Guard.AgainstNullOrWhiteSpace(ownerEntityName, nameof(ownerEntityName), 160);
        OwnerEntityId = Guard.AgainstEmpty(ownerEntityId, nameof(ownerEntityId));
        Status = StoredFileStatus.PendingScan;
    }

    public string StorageProvider { get; private set; }

    public string ContainerName { get; private set; }

    public string ObjectKey { get; private set; }

    public string OriginalFileName { get; private set; }

    public string ContentType { get; private set; }

    public long SizeBytes { get; private set; }

    public string Sha256Hash { get; private set; }

    public string OwnerEntityName { get; private set; }

    public Guid OwnerEntityId { get; private set; }

    public Guid? VersionEntityId { get; private set; }

    public StoredFileStatus Status { get; private set; }

    public void LinkVersion(Guid versionEntityId)
    {
        VersionEntityId = Guard.AgainstEmpty(versionEntityId, nameof(versionEntityId));
    }

    public void MarkAvailable()
    {
        if (Status == StoredFileStatus.Deleted)
        {
            throw new DomainException("Deleted files cannot be marked as available.");
        }

        Status = StoredFileStatus.Available;
    }

    public void Quarantine()
    {
        Status = StoredFileStatus.Quarantined;
    }

    public void Delete()
    {
        Status = StoredFileStatus.Deleted;
    }
}

public sealed class StorageProviderConfiguration : TenantEntity
{
    private StorageProviderConfiguration()
    {
        Name = string.Empty;
        ContainerName = string.Empty;
        SettingsJson = "{}";
    }

    public StorageProviderConfiguration(Guid tenantId, StorageProviderKind provider, string name, string containerName, int priority, bool isDefault, bool isEnabled, string settingsJson)
        : base(tenantId)
    {
        Provider = provider;
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 120);
        ContainerName = Guard.AgainstNullOrWhiteSpace(containerName, nameof(containerName), 180);
        Priority = Guard.AgainstOutOfRange(priority, nameof(priority), 1, 100);
        IsDefault = isDefault;
        IsEnabled = isEnabled;
        SettingsJson = string.IsNullOrWhiteSpace(settingsJson) ? "{}" : Guard.AgainstNullOrWhiteSpace(settingsJson, nameof(settingsJson), 8_000);
    }

    public StorageProviderKind Provider { get; private set; }

    public string Name { get; private set; }

    public string ContainerName { get; private set; }

    public int Priority { get; private set; }

    public bool IsDefault { get; private set; }

    public bool IsEnabled { get; private set; }

    public string SettingsJson { get; private set; }

    public DateTimeOffset? LastHealthCheckAtUtc { get; private set; }

    public bool LastHealthStatus { get; private set; }

    public string? LastHealthMessage { get; private set; }

    public void Update(string name, string containerName, int priority, bool isDefault, bool isEnabled, string settingsJson)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name), 120);
        ContainerName = Guard.AgainstNullOrWhiteSpace(containerName, nameof(containerName), 180);
        Priority = Guard.AgainstOutOfRange(priority, nameof(priority), 1, 100);
        IsDefault = isDefault;
        IsEnabled = isEnabled;
        SettingsJson = string.IsNullOrWhiteSpace(settingsJson) ? "{}" : Guard.AgainstNullOrWhiteSpace(settingsJson, nameof(settingsJson), 8_000);
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void MarkDefault()
    {
        IsDefault = true;
        IsEnabled = true;
    }

    public void RecordHealth(bool healthy, string message, DateTimeOffset checkedAtUtc)
    {
        LastHealthStatus = healthy;
        LastHealthMessage = Guard.AgainstNullOrWhiteSpace(message, nameof(message), 1_000);
        LastHealthCheckAtUtc = checkedAtUtc;
    }
}
