using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using WastePlatform.Application.Common.Interfaces;

namespace WastePlatform.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<string> SaveFileAsync(IFormFile file, string[] allowedExtensions, long maxSizeInBytes, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty.");
        }

        var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new InvalidOperationException($"Invalid file type: {fileExtension}");
        }

        if (file.Length > maxSizeInBytes)
        {
            throw new InvalidOperationException("File size exceeds limit.");
        }

        var fileName = $"{Guid.NewGuid()}{fileExtension}";
        
        var uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var filePath = Path.Combine(uploadsFolder, fileName);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        return fileName;
    }
}
