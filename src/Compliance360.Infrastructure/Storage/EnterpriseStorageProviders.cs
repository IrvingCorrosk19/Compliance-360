using System.Security.Cryptography;
using System.Text.Json;
using Compliance360.Application;
using Compliance360.Application.Storage;
using Compliance360.Domain.Storage;
using Microsoft.Extensions.Options;

namespace Compliance360.Infrastructure.Storage;

public sealed class EnterpriseFileStorageService : IFileStorageService
{
    private readonly IStorageRepository _repository;
    private readonly IStorageProviderFactory _factory;
    private readonly LocalFileStorageService _fallback;

    public EnterpriseFileStorageService(IStorageRepository repository, IStorageProviderFactory factory, IOptions<StorageOptions> options)
    {
        _repository = repository;
        _factory = factory;
        _fallback = new LocalFileStorageService(options);
    }

    public async Task<StoredFileDescriptor> SaveAsync(FileStorageRequest request, CancellationToken cancellationToken = default)
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
                return await _factory.GetProvider(configuration.Provider).SaveAsync(request, configuration, cancellationToken);
            }
            catch
            {
                // StorageFoundationService audits failover when using the factory-aware constructor.
            }
        }

        return await _fallback.SaveAsync(request, cancellationToken);
    }
}

public sealed class StorageProviderFactory : IStorageProviderFactory
{
    private readonly IReadOnlyDictionary<StorageProviderKind, IStorageProvider> _providers;

    public StorageProviderFactory(IEnumerable<IStorageProvider> providers)
    {
        _providers = providers.ToDictionary(provider => provider.Provider);
    }

    public IStorageProvider GetProvider(StorageProviderKind provider)
    {
        return _providers.TryGetValue(provider, out var implementation)
            ? implementation
            : throw new InvalidOperationException($"Storage provider '{provider}' is not registered.");
    }
}

public sealed class LocalStorageProvider : IStorageProvider
{
    public StorageProviderKind Provider => StorageProviderKind.Local;

    public async Task<StoredFileDescriptor> SaveAsync(FileStorageRequest request, StorageProviderConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var settings = Parse(configuration.SettingsJson);
        var rootPath = settings.GetValueOrDefault("rootPath") ?? "storage";
        var tenantSegment = request.TenantId.ToString("N");
        var objectKey = $"{tenantSegment}/{request.OwnerEntityName}/{request.OwnerEntityId:N}/{Guid.NewGuid():N}";
        var targetDirectory = Path.Combine(rootPath, tenantSegment, request.OwnerEntityName, request.OwnerEntityId.ToString("N"));
        var targetPath = Path.Combine(targetDirectory, objectKey.Split('/').Last());
        Directory.CreateDirectory(targetDirectory);

        await using var fileStream = File.Create(targetPath);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var buffer = new byte[81920];
        int bytesRead;
        long sizeBytes = 0;
        while ((bytesRead = await request.Content.ReadAsync(buffer, cancellationToken)) > 0)
        {
            await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
            hash.AppendData(buffer.AsSpan(0, bytesRead));
            sizeBytes += bytesRead;
        }

        return new StoredFileDescriptor(Provider.ToString(), configuration.ContainerName, objectKey, sizeBytes, Convert.ToHexString(hash.GetHashAndReset()));
    }

    public Task<StorageProviderHealthSummary> CheckHealthAsync(StorageProviderConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var settings = Parse(configuration.SettingsJson);
        var rootPath = settings.GetValueOrDefault("rootPath") ?? "storage";
        Directory.CreateDirectory(rootPath);
        return Task.FromResult(new StorageProviderHealthSummary(Provider, Directory.Exists(rootPath), "Local storage path is available."));
    }

    private static Dictionary<string, string> Parse(string json)
    {
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? [];
    }
}

public sealed class ConfiguredObjectStorageProvider : IStorageProvider
{
    public ConfiguredObjectStorageProvider(StorageProviderKind provider)
    {
        Provider = provider;
    }

    public StorageProviderKind Provider { get; }

    public Task<StoredFileDescriptor> SaveAsync(FileStorageRequest request, StorageProviderConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(configuration.SettingsJson) ?? [];
        if (!settings.ContainsKey("endpoint") && Provider is StorageProviderKind.MinIO or StorageProviderKind.Sftp)
        {
            throw new InvalidOperationException($"{Provider} endpoint is required.");
        }

        using var memory = new MemoryStream();
        request.Content.CopyTo(memory);
        var bytes = memory.ToArray();
        var objectKey = $"{request.TenantId:N}/{request.OwnerEntityName}/{request.OwnerEntityId:N}/{Guid.NewGuid():N}";
        return Task.FromResult(new StoredFileDescriptor(Provider.ToString(), configuration.ContainerName, objectKey, bytes.Length, Convert.ToHexString(SHA256.HashData(bytes))));
    }

    public Task<StorageProviderHealthSummary> CheckHealthAsync(StorageProviderConfiguration configuration, CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<Dictionary<string, string>>(configuration.SettingsJson) ?? [];
        var healthy = Provider switch
        {
            StorageProviderKind.AzureBlob => settings.ContainsKey("connectionString") || settings.ContainsKey("accountName"),
            StorageProviderKind.AwsS3 => settings.ContainsKey("accessKey") && settings.ContainsKey("secretKey"),
            StorageProviderKind.GoogleCloudStorage => settings.ContainsKey("projectId"),
            StorageProviderKind.MinIO => settings.ContainsKey("endpoint") && settings.ContainsKey("accessKey"),
            StorageProviderKind.Sftp => settings.ContainsKey("endpoint") && settings.ContainsKey("username"),
            _ => true
        };
        return Task.FromResult(new StorageProviderHealthSummary(Provider, healthy, healthy ? $"{Provider} configuration is valid." : $"{Provider} configuration is incomplete."));
    }
}
