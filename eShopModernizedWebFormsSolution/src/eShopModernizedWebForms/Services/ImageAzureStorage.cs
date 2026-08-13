using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using eShopModernizedWebForms.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;

namespace eShopModernizedWebForms.Services
{
    public class ImageAzureStorage : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _baseUrl;

        public ImageAzureStorage(IConfiguration configuration)
        {
            var connectionString = configuration["StorageConnectionString"];
            _blobServiceClient = new BlobServiceClient(connectionString);
            _baseUrl = _blobServiceClient.Uri.ToString().TrimEnd('/') + "/";
        }

        public string BaseUrl()
        {
            return _baseUrl;
        }

        public string BuildUrlImage(CatalogItem item)
        {
            if (string.IsNullOrEmpty(item.PictureFileName))
                return UrlDefaultImage();

            return _baseUrl + "pics/" + item.Id + "/" + item.PictureFileName;
        }

        public void Dispose()
        {
        }

        public void InitializeCatalogImages()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("pics");
            containerClient.CreateIfNotExists(PublicAccessType.Blob);

            // Delete existing blobs
            var blobs = containerClient.GetBlobs().ToList();
            foreach (var blob in blobs)
            {
                containerClient.DeleteBlobIfExists(blob.Name);
            }

            var webRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Pics");

            for (int i = 1; i <= 12; i++)
            {
                var path = Path.Combine(webRoot, i + ".png");
                if (File.Exists(path))
                {
                    var blobName = i + "/" + i + ".png";
                    UploadImageFromFile(containerClient, blobName, path, "image/png");
                }
            }

            var defaultImagePath = Path.Combine(webRoot, "default.png");
            if (File.Exists(defaultImagePath))
            {
                UploadImageFromFile(containerClient, "temp/default.png", defaultImagePath, "image/png");
            }
        }

        public void UpdateImage(CatalogItem item)
        {
            if (string.IsNullOrEmpty(item.TempImageName))
                return;

            var containerClient = _blobServiceClient.GetBlobContainerClient("pics");

            var folder = item.TempImageName.Replace("/pics/", string.Empty);
            var tempBlob = containerClient.GetBlobClient(folder);

            // Delete existing blobs for this item
            var blobs = containerClient.GetBlobs(prefix: item.Id + "/").ToList();
            foreach (var blob in blobs)
            {
                containerClient.DeleteBlobIfExists(blob.Name);
            }

            var fileName = Path.GetFileName(item.TempImageName);
            var imageBlob = containerClient.GetBlobClient(item.Id + "/" + fileName);

            imageBlob.StartCopyFromUri(tempBlob.Uri);
            tempBlob.DeleteIfExists();
        }

        public string UploadTempImage(IFormFile file, int? catalogItemId)
        {
            string path = catalogItemId.HasValue ? catalogItemId + "/temp/" : "temp/" + Guid.NewGuid().ToString() + "/";

            var containerClient = _blobServiceClient.GetBlobContainerClient("pics");
            var blobClient = containerClient.GetBlobClient(path + file.FileName.ToLower());

            using var stream = file.OpenReadStream();
            blobClient.Upload(stream, new BlobHttpHeaders { ContentType = file.ContentType });

            return blobClient.Uri.ToString();
        }

        public string UrlDefaultImage()
        {
            return _baseUrl + "pics/temp/default.png";
        }

        private void UploadImageFromFile(BlobContainerClient container, string blobName, string filePath, string contentType)
        {
            var blobClient = container.GetBlobClient(blobName);
            using var fileStream = File.OpenRead(filePath);
            blobClient.Upload(fileStream, new BlobHttpHeaders { ContentType = contentType });
        }
    }
}
