using System.Security.Cryptography;
using System.Text;
using Compliance360.Application;
using Compliance360.Application.Storage;
using Compliance360.Domain.Audit;
using Compliance360.Domain.Common;
using Compliance360.Domain.Storage;
using Compliance360.Infrastructure.Persistence;
using Compliance360.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Compliance360.Tests;

public sealed class StorageFoundationTests
{
    [Fact]
    public async Task UploadAsync_Saves_Metadata_And_Audit()
    {
        var fixture = StorageFixture.Create();
        var ownerId = Guid.NewGuid();

        var result = await fixture.Service.UploadAsync(CreateUpload(fixture.TenantId, fixture.UserId, ownerId, "hello"));

        Assert.True(result.IsSuccess);
        Assert.Equal(fixture.TenantId, result.Value!.TenantId);
        Assert.Equal("document.pdf", result.Value.OriginalFileName);
        Assert.Equal(StoredFileStatus.PendingScan, result.Value.Status);
        Assert.Single(fixture.Repository.StoredFiles);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.FileUploaded);
    }

    [Fact]
    public async Task UploadAsync_Rejects_Invalid_Metadata()
    {
        var fixture = StorageFixture.Create();

        var result = await fixture.Service.UploadAsync(CreateUpload(Guid.Empty, fixture.UserId, Guid.NewGuid(), "hello"));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task GetAsync_Enforces_Tenant_Isolation()
    {
        var fixture = StorageFixture.Create();
        var upload = await fixture.Service.UploadAsync(CreateUpload(fixture.TenantId, fixture.UserId, Guid.NewGuid(), "hello"));

        var wrongTenant = await fixture.Service.GetAsync(new GetStoredFileQuery(Guid.NewGuid(), upload.Value!.Id, fixture.UserId));

        Assert.True(wrongTenant.IsFailure);
    }

    [Fact]
    public async Task MarkAvailable_Then_RegisterDownload_Audits_Download()
    {
        var fixture = StorageFixture.Create();
        var upload = await fixture.Service.UploadAsync(CreateUpload(fixture.TenantId, fixture.UserId, Guid.NewGuid(), "hello"));

        var available = await fixture.Service.MarkAvailableAsync(new ChangeStoredFileStatusCommand(fixture.TenantId, upload.Value!.Id, fixture.UserId));
        var download = await fixture.Service.RegisterDownloadAsync(new RegisterFileDownloadCommand(fixture.TenantId, upload.Value.Id, fixture.UserId));

        Assert.True(available.IsSuccess);
        Assert.True(download.IsSuccess);
        Assert.Equal(upload.Value.Sha256Hash, download.Value!.Sha256Hash);
        Assert.Contains(fixture.Repository.AuditLogs, audit => audit.Action == AuditAction.FileDownloaded);
    }

    [Fact]
    public async Task Quarantine_And_Delete_Prevent_Download()
    {
        var fixture = StorageFixture.Create();
        var upload = await fixture.Service.UploadAsync(CreateUpload(fixture.TenantId, fixture.UserId, Guid.NewGuid(), "hello"));

        var quarantine = await fixture.Service.QuarantineAsync(new ChangeStoredFileStatusCommand(fixture.TenantId, upload.Value!.Id, fixture.UserId));
        var quarantinedDownload = await fixture.Service.RegisterDownloadAsync(new RegisterFileDownloadCommand(fixture.TenantId, upload.Value.Id, fixture.UserId));
        var delete = await fixture.Service.DeleteAsync(new ChangeStoredFileStatusCommand(fixture.TenantId, upload.Value.Id, fixture.UserId));
        var deletedDownload = await fixture.Service.RegisterDownloadAsync(new RegisterFileDownloadCommand(fixture.TenantId, upload.Value.Id, fixture.UserId));

        Assert.True(quarantine.IsSuccess);
        Assert.True(quarantinedDownload.IsFailure);
        Assert.True(delete.IsSuccess);
        Assert.True(deletedDownload.IsFailure);
    }

    [Fact]
    public async Task Deleted_File_Cannot_Be_Marked_Available()
    {
        var fixture = StorageFixture.Create();
        var upload = await fixture.Service.UploadAsync(CreateUpload(fixture.TenantId, fixture.UserId, Guid.NewGuid(), "hello"));
        await fixture.Service.DeleteAsync(new ChangeStoredFileStatusCommand(fixture.TenantId, upload.Value!.Id, fixture.UserId));

        var result = await fixture.Service.MarkAvailableAsync(new ChangeStoredFileStatusCommand(fixture.TenantId, upload.Value.Id, fixture.UserId));

        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task LocalFileStorageService_Writes_File_And_Computes_Sha256()
    {
        var root = Path.Combine(Path.GetTempPath(), $"compliance360-storage-{Guid.NewGuid():N}");
        var service = new LocalFileStorageService(Options.Create(new StorageOptions { RootPath = root, Provider = "Local", ContainerName = "docs" }));
        var bytes = Encoding.UTF8.GetBytes("hello");

        var result = await service.SaveAsync(new FileStorageRequest(Guid.NewGuid(), "document.pdf", "application/pdf", new MemoryStream(bytes), "Document", Guid.NewGuid()));

        Assert.Equal("Local", result.StorageProvider);
        Assert.Equal("docs", result.ContainerName);
        Assert.Equal(bytes.Length, result.SizeBytes);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(bytes)), result.Sha256Hash);
        Assert.True(Directory.Exists(root));
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task EfStorageRepository_Persists_And_Loads_By_Tenant()
    {
        await using var dbContext = CreateDbContext();
        var repository = new EfStorageRepository(dbContext);
        var tenantId = Guid.NewGuid();
        var storedFile = CreateStoredFile(tenantId);
        var audit = AuditLog.Create(tenantId, Guid.NewGuid(), nameof(StoredFile), storedFile.Id, AuditAction.FileUploaded, DateTimeOffset.UtcNow);

        await repository.AddAsync(storedFile);
        await repository.AddAuditLogAsync(audit);
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(tenantId, storedFile.Id);
        var wrongTenant = await repository.GetByIdAsync(Guid.NewGuid(), storedFile.Id);

        Assert.NotNull(loaded);
        Assert.Null(wrongTenant);
        Assert.Single(dbContext.AuditLogs);
    }

    [Fact]
    public void StoredFile_Domain_Rules_Handle_Status_And_Version()
    {
        var storedFile = CreateStoredFile(Guid.NewGuid());
        var versionId = Guid.NewGuid();

        storedFile.LinkVersion(versionId);
        storedFile.MarkAvailable();
        storedFile.Quarantine();
        storedFile.Delete();

        Assert.Equal(versionId, storedFile.VersionEntityId);
        Assert.Equal(StoredFileStatus.Deleted, storedFile.Status);
        Assert.Throws<DomainException>(() => new StoredFile(Guid.NewGuid(), "Local", "docs", "key", "file.pdf", "application/pdf", 0, "hash", "Document", Guid.NewGuid()));
    }

    [Fact]
    public async Task Storage_Provider_Administration_Is_Tenant_Scoped_And_Testable()
    {
        var fixture = StorageFixture.Create();
        var created = await fixture.Service.CreateProviderAsync(new CreateStorageProviderConfigurationCommand(fixture.TenantId, fixture.UserId, StorageProviderKind.Local, "Local primary", "docs", 1, true, true, "{\"rootPath\":\"storage-test\"}"));

        var listed = await fixture.Service.ListProvidersAsync(fixture.TenantId);
        var tested = await fixture.Service.TestProviderAsync(new ChangeStorageProviderCommand(fixture.TenantId, fixture.UserId, created.Value!.Id));
        var wrongTenant = await fixture.Service.ListProvidersAsync(Guid.NewGuid());

        Assert.True(created.IsSuccess);
        Assert.Single(listed.Value!);
        Assert.True(tested.Value!.Healthy);
        Assert.Empty(wrongTenant.Value!);
    }

    private static UploadFileCommand CreateUpload(Guid tenantId, Guid userId, Guid ownerId, string content)
    {
        return new UploadFileCommand(
            tenantId,
            userId,
            "document.pdf",
            "application/pdf",
            new MemoryStream(Encoding.UTF8.GetBytes(content)),
            "Document",
            ownerId,
            Guid.NewGuid());
    }

    private static StoredFile CreateStoredFile(Guid tenantId)
    {
        return new StoredFile(tenantId, "Local", "docs", Guid.NewGuid().ToString("N"), "file.pdf", "application/pdf", 10, "hash", "Document", Guid.NewGuid());
    }

    private static Compliance360DbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<Compliance360DbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new Compliance360DbContext(options, new FixedClock());
    }

    private sealed class StorageFixture
    {
        private StorageFixture()
        {
            TenantId = Guid.NewGuid();
            UserId = Guid.NewGuid();
            Repository = new InMemoryStorageRepository();
            var providerFactory = new StorageProviderFactory([new LocalStorageProvider()]);
            Service = new StorageFoundationService(Repository, new FakeApplicationDbContext(), new FakeFileStorageService(), providerFactory, new FixedClock());
        }

        public Guid TenantId { get; }
        public Guid UserId { get; }
        public InMemoryStorageRepository Repository { get; }
        public StorageFoundationService Service { get; }

        public static StorageFixture Create()
        {
            return new StorageFixture();
        }
    }

    private sealed class InMemoryStorageRepository : IStorageRepository
    {
        public List<StoredFile> StoredFiles { get; } = [];
        public List<StorageProviderConfiguration> ProviderConfigurations { get; } = [];
        public List<AuditLog> AuditLogs { get; } = [];

        public Task AddAsync(StoredFile storedFile, CancellationToken cancellationToken = default)
        {
            StoredFiles.Add(storedFile);
            return Task.CompletedTask;
        }

        public Task<StoredFile?> GetByIdAsync(Guid tenantId, Guid storedFileId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(StoredFiles.SingleOrDefault(file => file.TenantId == tenantId && file.Id == storedFileId));
        }

        public Task AddProviderConfigurationAsync(StorageProviderConfiguration configuration, CancellationToken cancellationToken = default)
        {
            ProviderConfigurations.Add(configuration);
            return Task.CompletedTask;
        }

        public Task<StorageProviderConfiguration?> GetProviderConfigurationAsync(Guid tenantId, Guid providerConfigurationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ProviderConfigurations.SingleOrDefault(configuration => configuration.TenantId == tenantId && configuration.Id == providerConfigurationId));
        }

        public Task<IReadOnlyCollection<StorageProviderConfiguration>> ListProviderConfigurationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<StorageProviderConfiguration>>(ProviderConfigurations.Where(configuration => configuration.TenantId == tenantId).ToArray());
        }

        public Task AddAuditLogAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
        {
            AuditLogs.Add(auditLog);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeFileStorageService : IFileStorageService
    {
        public async Task<StoredFileDescriptor> SaveAsync(FileStorageRequest request, CancellationToken cancellationToken = default)
        {
            using var memory = new MemoryStream();
            await request.Content.CopyToAsync(memory, cancellationToken);
            var bytes = memory.ToArray();
            return new StoredFileDescriptor("Fake", "docs", $"{request.TenantId:N}/{Guid.NewGuid():N}", bytes.Length, Convert.ToHexString(SHA256.HashData(bytes)));
        }

        public Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken = default)
            => throw new FileNotFoundException("Fake storage has no persisted content.");
    }

    private sealed class FakeApplicationDbContext : IApplicationDbContext
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 6, 20, 18, 0, 0, TimeSpan.Zero);
    }
}
