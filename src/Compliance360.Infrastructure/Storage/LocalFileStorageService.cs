using System.Security.Cryptography;
using Compliance360.Application;
using Microsoft.Extensions.Options;

namespace Compliance360.Infrastructure.Storage;

public sealed class LocalFileStorageService : IFileStorageService
{
    private readonly StorageOptions _options;

    public LocalFileStorageService(IOptions<StorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<StoredFileDescriptor> SaveAsync(FileStorageRequest request, CancellationToken cancellationToken = default)
    {
        var tenantSegment = request.TenantId.ToString("N");
        var objectKey = $"{tenantSegment}/{request.OwnerEntityName}/{request.OwnerEntityId:N}/{Guid.NewGuid():N}";
        var targetDirectory = Path.Combine(_options.RootPath, tenantSegment, request.OwnerEntityName, request.OwnerEntityId.ToString("N"));
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

        var sha256Hash = Convert.ToHexString(hash.GetHashAndReset());

        return new StoredFileDescriptor(
            _options.Provider,
            _options.ContainerName,
            objectKey,
            sizeBytes,
            sha256Hash);
    }

    public Task<Stream> OpenReadAsync(string objectKey, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(objectKey))
        {
            throw new FileNotFoundException("Stored file object key is missing.");
        }

        var relative = objectKey.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var candidates = new[]
        {
            Path.Combine(_options.RootPath, relative),
            Path.GetFullPath(Path.Combine(_options.RootPath, relative)),
            Path.Combine("/app/storage", relative),
            Path.Combine("/app/web/storage", relative)
        };

        foreach (var candidate in candidates.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (File.Exists(candidate))
            {
                Stream stream = File.OpenRead(candidate);
                return Task.FromResult(stream);
            }
        }

        throw new FileNotFoundException($"Stored file content was not found for object key '{objectKey}'.");
    }
}