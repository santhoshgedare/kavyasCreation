using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Web.Services
{
    /// <summary>
    /// Service for managing file uploads to Azure Blob Storage
    /// </summary>
    public interface IBlobStorageService
    {
        Task<string> UploadAsync(IFormFile file, string containerName = "product-images");
        Task<bool> DeleteAsync(string blobUri, string containerName = "product-images");
        Task<Stream> DownloadAsync(string blobName, string containerName = "product-images");
    }

    public class BlobStorageService : IBlobStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ILogger<BlobStorageService> _logger;
        private const string ProductContainer = "product-images";

        public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _logger = logger;
        }

        /// <summary>
        /// Uploads a file to Azure Blob Storage
        /// </summary>
        public async Task<string> UploadAsync(IFormFile file, string containerName = "product-images")
        {
            try
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("File is empty", nameof(file));

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                var blobClient = containerClient.GetBlobClient(blobName);

                await using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, overwrite: true);

                _logger.LogInformation("File uploaded successfully: {BlobName} to container: {ContainerName}", blobName, containerName);
                return blobClient.Uri.AbsoluteUri;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to Azure Blob Storage");
                throw;
            }
        }

        /// <summary>
        /// Deletes a blob from Azure Blob Storage
        /// </summary>
        public async Task<bool> DeleteAsync(string blobUri, string containerName = "product-images")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(blobUri))
                    return false;

                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                
                // Extract blob name from URI
                var uri = new Uri(blobUri);
                var blobName = uri.Segments.Last();

                var blobClient = containerClient.GetBlobClient(blobName);
                await blobClient.DeleteIfExistsAsync();

                _logger.LogInformation("Blob deleted successfully: {BlobUri}", blobUri);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting blob from Azure Blob Storage: {BlobUri}", blobUri);
                return false;
            }
        }

        /// <summary>
        /// Downloads a blob from Azure Blob Storage
        /// </summary>
        public async Task<Stream> DownloadAsync(string blobName, string containerName = "product-images")
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                BlobDownloadInfo download = await blobClient.DownloadAsync();
                return download.Content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading blob from Azure Blob Storage: {BlobName}", blobName);
                throw;
            }
        }
    }
}
