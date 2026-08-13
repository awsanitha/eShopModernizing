using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using eShopModernizedMVC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace eShopModernizedMVC.Services
{
    public class ImageAzureStorage : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "pics";

        public ImageAzureStorage()
        {
            _blobServiceClient = new BlobServiceClient(CatalogConfiguration.StorageConnectionString);
        }

        public string BaseUrl()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            return containerClient.Uri.ToString() + "/";
        }

        public string BuildUrlImage(CatalogItem item)
        {
            if (string.IsNullOrEmpty(item.PictureFileName))
                return UrlDefaultImage();
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            return containerClient.Uri + "/" + item.Id + "/" + item.PictureFileName;
        }

        public void Dispose()
        {
        }

        public void InitializeCatalogImages()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            containerClient.CreateIfNotExists(PublicAccessType.Blob);

            // Delete existing blobs
            foreach (var blob in containerClient.GetBlobs())
            {
                containerClient.GetBlobClient(blob.Name).Delete();
            }
        }

        public void UpdateImage(CatalogItem item)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var folder = item.TempImageName?.Replace("/pics/", string.Empty) ?? string.Empty;
            var tempBlob = containerClient.GetBlobClient(folder);

            // Delete existing blobs for item
            foreach (var blob in containerClient.GetBlobs(prefix: item.Id + "/"))
            {
                containerClient.GetBlobClient(blob.Name).Delete();
            }

            if (!string.IsNullOrEmpty(folder))
            {
                var fileName = Path.GetFileName(item.TempImageName);
                var targetBlob = containerClient.GetBlobClient(item.Id + "/" + fileName);
                targetBlob.StartCopyFromUri(tempBlob.Uri);
                tempBlob.Delete();
            }
        }

        public async Task<string> UploadTempImageAsync(IFormFile file, int? catalogItemId)
        {
            string path = catalogItemId.HasValue ? catalogItemId + "/temp/" : "temp/" + Guid.NewGuid().ToString() + "/";
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(path + file.FileName.ToLower());

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public string UrlDefaultImage()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            return containerClient.Uri + "/temp/default.png";
        }
    }
}
