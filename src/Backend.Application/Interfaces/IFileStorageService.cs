using Microsoft.AspNetCore.Http;

namespace Backend.Application.Interfaces;
public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string subDirectory, CancellationToken cancellationToken);
}