using Microsoft.AspNetCore.Http;

namespace WastePlatform.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string[] allowedExtensions, long maxSizeInBytes, CancellationToken cancellationToken = default);
}
