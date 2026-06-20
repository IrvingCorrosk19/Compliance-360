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
}
