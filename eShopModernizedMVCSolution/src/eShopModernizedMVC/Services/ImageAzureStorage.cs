using eShopModernizedMVC.Models;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace eShopModernizedMVC.Services
{
    public class ImageAzureStorage : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ImageAzureStorage(IWebHostEnvironment hostEnvironment)
        {
            _hostEnvironment = hostEnvironment;
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

        public void InitializeCatalogImages()
        {
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient("pics");

            container.CreateIfNotExists(PublicAccessType.Blob);

            Parallel.ForEach(container.GetBlobs(), blobItem => container.DeleteBlobIfExists(blobItem.Name));

            var webRoot = Path.Combine(_hostEnvironment.WebRootPath, "Pics");

            for (int i = 1; i <= 12; i++)
            {
                var path = Path.Combine(webRoot, i + ".png");
                var blobName = i + "/" + i + ".png";
                UpLoadImageFromFile(container, blobName, path, "image/png");
            }
            var defaultImagePath = Path.Combine(webRoot, "default.png");
            UpLoadImageFromFile(container, "temp/default.png", defaultImagePath, "image/png");
        }

        public void UpdateImage(CatalogItem item)
        {
            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient("pics");

            var folder = item.TempImageName.Replace("/pics/", string.Empty);

            BlobClient tempBlob = container.GetBlobClient(folder);

            var blockBlobs = container.GetBlobs(prefix: item.Id + "/");
            foreach (var blockBlob in blockBlobs)
            {
                container.DeleteBlobIfExists(blockBlob.Name);
            }

            var fileName = Path.GetFileName(item.TempImageName);
            BlobClient imageBlob = container.GetBlobClient(item.Id + "/" + fileName);

            imageBlob.StartCopyFromUri(tempBlob.Uri);
            tempBlob.DeleteIfExists();
        }

        public string UploadTempImage(IFormFile file, int? catalogItemId)
        {
            string path = catalogItemId.HasValue ? catalogItemId + "/temp/" : "temp/" + Guid.NewGuid().ToString() + "/";

            BlobContainerClient container = _blobServiceClient.GetBlobContainerClient("pics");
            BlobClient blockBlob = container.GetBlobClient(path + file.FileName.ToLower());

            using (var stream = file.OpenReadStream())
            {
                blockBlob.Upload(stream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = file.ContentType }
                });
            }

            return blockBlob.Uri.ToString();
        }

        public string UrlDefaultImage()
        {
            return _blobServiceClient.Uri + "pics/temp/default.png";
        }

        private void UpLoadImageFromFile(BlobContainerClient container, string blobName, string filePath, string contentType)
        {
            using (var fileStream = File.OpenRead(filePath))
            {
                BlobClient blockBlob = container.GetBlobClient(blobName);
                blockBlob.Upload(fileStream, new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
                });
            }
        }
    }
}
