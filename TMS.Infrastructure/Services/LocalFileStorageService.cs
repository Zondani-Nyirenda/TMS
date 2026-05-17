using Microsoft.Extensions.Configuration;
using TMS.Application.Interfaces;

namespace TMS.Infrastructure.Services;

/// <summary>
/// Stores files on the local filesystem under a configurable root.
/// Set "FileStorage:RootPath" in appsettings.json.
/// Replace with an Azure Blob or S3 implementation without touching any other layer.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _root;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _root = configuration["FileStorage:RootPath"]
             ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        Directory.CreateDirectory(_root);
    }

    public async Task<string> SaveFileAsync(
        string fileBase64, string fileName,
        string folder = "content", CancellationToken ct = default)
    {
        var bytes = Convert.FromBase64String(fileBase64);

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        var safeName = $"{Guid.NewGuid():N}{ext}";
        var relative = Path.Combine(folder, safeName).Replace('\\', '/');
        var absolute = Path.Combine(_root, relative.Replace('/', Path.DirectorySeparatorChar));

        Directory.CreateDirectory(Path.GetDirectoryName(absolute)!);

        await File.WriteAllBytesAsync(absolute, bytes, ct);

        return relative;
    }

    public Task DeleteFileAsync(string storagePath, CancellationToken ct = default)
    {
        var absolute = Resolve(storagePath);
        if (File.Exists(absolute)) File.Delete(absolute);
        return Task.CompletedTask;
    }

    public Task<string> GetFileUrl(string storagePath)
        => Task.FromResult($"/api/content/stream?path={Uri.EscapeDataString(storagePath)}");

    public Task<Stream> OpenReadAsync(string storagePath, CancellationToken ct = default)
    {
        var absolute = Resolve(storagePath);

        if (!File.Exists(absolute))
            throw new FileNotFoundException($"Storage file not found: {storagePath}");

        Stream fs = File.OpenRead(absolute);
        return Task.FromResult(fs);
    }

    public Task<bool> ExistsAsync(string storagePath, CancellationToken ct = default)
        => Task.FromResult(File.Exists(Resolve(storagePath)));

    // ── Private ───────────────────────────────────────────────────────────────

    private string Resolve(string storagePath)
        => Path.Combine(_root, storagePath.Replace('/', Path.DirectorySeparatorChar));
}