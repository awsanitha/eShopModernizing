using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using eShopModernizedWebForms.Models;

namespace eShopModernizedWebForms.Services
{
    public class ImageAzureStorage : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string ContainerName = "pics";

        public ImageAzureStorage()
        {
            var connectionString = CatalogConfiguration.StorageConnectionString;
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        public string BaseUrl()
        {
            return _blobServiceClient.Uri.ToString();
        }

        public string BuildUrlImage(CatalogItem item)
        {
            if (string.IsNullOrEmpty(item.PictureFileName))
                return UrlDefaultImage();

            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient($"{item.Id}/{item.PictureFileName}");
            return blobClient.Uri.ToString();
        }

        public void Dispose() { }

        public void InitializeCatalogImages()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            containerClient.CreateIfNotExists(PublicAccessType.Blob);

            // Delete existing blobs
            foreach (var blobItem in containerClient.GetBlobs())
            {
                containerClient.DeleteBlob(blobItem.Name);
            }

            // Upload default pics from the app directory
            var webRoot = AppDomain.CurrentDomain.BaseDirectory;
            var picsPath = Path.Combine(webRoot, "Pics");

            if (!Directory.Exists(picsPath))
                return;

            for (int i = 1; i <= 12; i++)
            {
                var path = Path.Combine(picsPath, i + ".png");
                if (File.Exists(path))
                {
                    var blobName = $"{i}/{i}.png";
                    var blobClient = containerClient.GetBlobClient(blobName);
                    using var fileStream = File.OpenRead(path);
                    blobClient.Upload(fileStream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = "image/png" } });
                }
            }

            var defaultImagePath = Path.Combine(picsPath, "default.png");
            if (File.Exists(defaultImagePath))
            {
                var defaultBlobClient = containerClient.GetBlobClient("temp/default.png");
                using var fileStream = File.OpenRead(defaultImagePath);
                defaultBlobClient.Upload(fileStream, new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = "image/png" } });
            }
        }

        public void UpdateImage(CatalogItem item)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);

            // The temp image name stores the path like "/pics/temp/xxx.png"
            var folder = item.TempImageName?.Replace("/pics/", string.Empty) ?? string.Empty;
            var tempBlobClient = containerClient.GetBlobClient(folder);

            // Delete existing blobs in the item's folder
            foreach (var blobItem in containerClient.GetBlobs(prefix: item.Id + "/"))
            {
                containerClient.DeleteBlob(blobItem.Name);
            }

            var fileName = Path.GetFileName(item.TempImageName ?? string.Empty);
            var destBlobClient = containerClient.GetBlobClient($"{item.Id}/{fileName}");

            // Copy from temp to destination
            if (tempBlobClient.Exists())
            {
                destBlobClient.StartCopyFromUri(tempBlobClient.Uri);
                tempBlobClient.Delete();
            }
        }

        public string UploadTempImage(IFormFile file, int? catalogItemId)
        {
            string path = catalogItemId.HasValue
                ? $"{catalogItemId}/temp/"
                : $"temp/{Guid.NewGuid()}/";

            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            var blobClient = containerClient.GetBlobClient(path + file.FileName.ToLower());

            using var stream = file.OpenReadStream();
            blobClient.Upload(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
            });

            return blobClient.Uri.ToString();
        }

        public string UrlDefaultImage()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
            return containerClient.GetBlobClient("temp/default.png").Uri.ToString();
        }
    }
}
