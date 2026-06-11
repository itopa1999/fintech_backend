using Backend.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string subDirectory, CancellationToken cancellationToken)
    {
        // Create directory if not exists
        var uploadDir = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "Media", subDirectory);
        Directory.CreateDirectory(uploadDir);

        // Generate unique file name
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(uploadDir, fileName);

        // Save file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        // Return relative URL (or absolute)
        var relativeUrl = $"/Media/{subDirectory}/{fileName}";
        _logger.LogInformation("File saved locally: {RelativeUrl}", relativeUrl);
        return relativeUrl;
    }
}