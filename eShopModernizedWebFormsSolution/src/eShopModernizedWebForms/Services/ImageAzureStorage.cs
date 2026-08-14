using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using eShopModernizedWebForms.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace eShopModernizedWebForms.Services
{
    public class ImageAzureStorage : IImageService
    {
        private readonly BlobServiceClient _serviceClient;
        private readonly string webRootPath;

        public ImageAzureStorage(string webRootPath)
        {
            _serviceClient = new BlobServiceClient(CatalogConfiguration.StorageConnectionString);
            this.webRootPath = webRootPath;
        }

        public string BaseUrl()
        {
            return _serviceClient.Uri.ToString();
        }

        public string BuildUrlImage(CatalogItem item)
        {
            if (string.IsNullOrEmpty(item.PictureFileName))
                return UrlDefaultImage();

            return _serviceClient.Uri + "pics/" + item.Id + "/" + item.PictureFileName;
        }

        public void Dispose()
        {
        }

        public void InitializeCatalogImages()
        {
            BlobContainerClient container = _serviceClient.GetBlobContainerClient("pics");

            container.CreateIfNotExists(PublicAccessType.Blob);

            Parallel.ForEach(container.GetBlobs(), blobItem =>
            {
                container.GetBlobClient(blobItem.Name).DeleteIfExists();
            });

            var picsRoot = Path.Combine(webRootPath, "Pics");

            for (int i = 1; i <= 12; i++)
            {
                var path = Path.Combine(picsRoot, i + ".png");
                var blobName = i + "/" + i + ".png";
                UpLoadImageFromFile(container, blobName, path, "image/png");
            }
            var defaultImagePath = Path.Combine(picsRoot, "default.png");
            UpLoadImageFromFile(container, "temp/default.png", defaultImagePath, "image/png");
        }

        public void UpdateImage(CatalogItem item)
        {
            BlobContainerClient container = _serviceClient.GetBlobContainerClient("pics");

            var folder = item.TempImageName.Replace("/pics/", string.Empty);

            BlobClient tempBlob = container.GetBlobClient(folder);

            var blockBlobs = container.GetBlobs(prefix: item.Id + "/");
            foreach (var blockBlob in blockBlobs)
            {
                container.GetBlobClient(blockBlob.Name).DeleteIfExists();
            }

            var fileName = Path.GetFileName(item.TempImageName);
            BlobClient imageBlob = container.GetBlobClient(item.Id + "/" + fileName);

            imageBlob.StartCopyFromUri(tempBlob.Uri);
            tempBlob.DeleteIfExists();
        }

        public string UploadTempImage(IFormFile file, int? catalogItemId)
        {
            string path = catalogItemId.HasValue ? catalogItemId + "/temp/" : "temp/" + Guid.NewGuid().ToString() + "/";

            BlobContainerClient container = _serviceClient.GetBlobContainerClient("pics");
            BlobClient blobClient = container.GetBlobClient(path + file.FileName.ToLower());

            using (var stream = file.OpenReadStream())
            {
                blobClient.Upload(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return blobClient.Uri.ToString();
        }

        public string UrlDefaultImage()
        {
            return _serviceClient.Uri + "pics/temp/default.png";
        }

        private void UpLoadImageFromFile(BlobContainerClient container, string blobName, string filePath, string contentType)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                BlobClient blobClient = container.GetBlobClient(blobName);
                blobClient.Upload(fileStream, new BlobHttpHeaders { ContentType = contentType }, conditions: null);
            }
        }
    }
}
