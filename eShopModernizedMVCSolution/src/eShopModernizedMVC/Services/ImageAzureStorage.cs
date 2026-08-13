using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using eShopModernizedMVC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

namespace eShopModernizedMVC.Services
{
    public class ImageAzureStorage : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public ImageAzureStorage()
        {
            _blobServiceClient = new BlobServiceClient(CatalogConfiguration.StorageConnectionString);
        }

        public string BaseUrl()
        {
            return _blobServiceClient.Uri.ToString();
        }

        public string BuildUrlImage(CatalogItem item)
        {
            if (string.IsNullOrEmpty(item.PictureFileName))
                return UrlDefaultImage();

            return _blobServiceClient.Uri + "pics/" + item.Id + "/" + item.PictureFileName;
        }

        public void Dispose()
        {
        }

        public void InitializeCatalogImages()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("pics");
            containerClient.CreateIfNotExists(PublicAccessType.Blob);

            // Delete existing blobs
            foreach (var blobItem in containerClient.GetBlobs())
            {
                containerClient.DeleteBlobIfExists(blobItem.Name);
            }

            var webRoot = AppDomain.CurrentDomain.BaseDirectory;
            var picsPath = Path.Combine(webRoot, "Pics");

            for (int i = 1; i <= 12; i++)
            {
                var path = Path.Combine(picsPath, i + ".png");
                var blobName = i + "/" + i + ".png";
                UpLoadImageFromFile(containerClient, blobName, path, "image/png");
            }
            var defaultImagePath = Path.Combine(picsPath, "default.png");
            UpLoadImageFromFile(containerClient, "temp/default.png", defaultImagePath, "image/png");
        }

        public void UpdateImage(CatalogItem item)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient("pics");

            var folder = item.TempImageName!.Replace("/pics/", string.Empty);
            var tempBlob = containerClient.GetBlobClient(folder);

            // Delete existing blobs for this item
            foreach (var blobItem in containerClient.GetBlobs(prefix: item.Id + "/"))
            {
                containerClient.DeleteBlobIfExists(blobItem.Name);
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

            using (var stream = file.OpenReadStream())
            {
                blobClient.Upload(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            }

            return blobClient.Uri.ToString();
        }

        public string UrlDefaultImage()
        {
            return _blobServiceClient.Uri + "pics/temp/default.png";
        }

        private void UpLoadImageFromFile(BlobContainerClient containerClient, string blobName, string filePath, string contentType)
        {
            if (!File.Exists(filePath)) return;
            var blobClient = containerClient.GetBlobClient(blobName);
            using var fileStream = File.OpenRead(filePath);
            blobClient.Upload(fileStream, new BlobHttpHeaders { ContentType = contentType });
        }
    }
}
