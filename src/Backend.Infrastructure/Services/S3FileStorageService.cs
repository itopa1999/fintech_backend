
// public class S3FileStorageService : IFileStorageService
// {
//     private readonly IAmazonS3 _s3Client;
//     private readonly string _bucketName;
//     private readonly ILogger<S3FileStorageService> _logger;

//     public S3FileStorageService(IAmazonS3 s3Client, IConfiguration config, ILogger<S3FileStorageService> logger)
//     {
//         _s3Client = s3Client;
//         _bucketName = config["AWS:BucketName"];
//         _logger = logger;
//     }

//     public async Task<string> UploadFileAsync(IFormFile file, string subDirectory, CancellationToken cancellationToken)
//     {
//         var key = $"{subDirectory}/{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

//         using var memoryStream = new MemoryStream();
//         await file.CopyToAsync(memoryStream, cancellationToken);
//         memoryStream.Position = 0;

//         var putRequest = new PutObjectRequest
//         {
//             BucketName = _bucketName,
//             Key = key,
//             InputStream = memoryStream,
//             ContentType = file.ContentType
//         };

//         var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);
//         if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
//             throw new Exception("Failed to upload file to S3");

//         // Return public URL (or generate presigned URL if bucket is private)
//         var url = $"https://{_bucketName}.s3.amazonaws.com/{key}";
//         _logger.LogInformation("File uploaded to S3: {Url}", url);
//         return url;
//     }
// }