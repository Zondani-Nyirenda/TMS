// TMS.Application/Interfaces/IFileStorageService.cs
namespace TMS.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(
        string fileBase64,
        string fileName,
        string folder = "content",
        CancellationToken ct = default);

    Task DeleteFileAsync(
        string storagePath,
        CancellationToken ct = default);

    Task<string> GetFileUrl(string storagePath);

    Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken ct = default);

    Task<bool> ExistsAsync(
        string storagePath,
        CancellationToken ct = default);
}