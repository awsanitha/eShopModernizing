using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using eShopModernizedWebForms.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace eShopModernizedWebForms.Services
{
    public class ImageAzureStorage : IImageService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private const string _containerName = "pics";

        public ImageAzureStorage()
        {
            _blobServiceClient = new BlobServiceClient(CatalogConfiguration.StorageConnectionString);
        }

        public string BaseUrl()
        {
            return _blobServiceClient.GetBlobContainerClient(_containerName).Uri.ToString() + "/";
        }

        public string BuildUrlImage(CatalogItem item)
        {
            if (string.IsNullOrEmpty(item.PictureFileName)) return UrlDefaultImage();
            return _blobServiceClient.GetBlobContainerClient(_containerName).Uri + "/" + item.Id + "/" + item.PictureFileName;
        }

        public void Dispose() { }

        public void InitializeCatalogImages()
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            container.CreateIfNotExists(PublicAccessType.Blob);
            foreach (var blob in container.GetBlobs())
                container.GetBlobClient(blob.Name).Delete();
        }

        public void UpdateImage(CatalogItem item)
        {
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var folder = item.TempImageName?.Replace("/pics/", string.Empty) ?? string.Empty;
            var tempBlob = container.GetBlobClient(folder);
            foreach (var blob in container.GetBlobs(prefix: item.Id + "/"))
                container.GetBlobClient(blob.Name).Delete();
            if (!string.IsNullOrEmpty(folder))
            {
                var targetBlob = container.GetBlobClient(item.Id + "/" + Path.GetFileName(item.TempImageName));
                targetBlob.StartCopyFromUri(tempBlob.Uri);
                tempBlob.Delete();
            }
        }

        public async Task<string> UploadTempImageAsync(IFormFile file, int? catalogItemId)
        {
            string path = catalogItemId.HasValue ? catalogItemId + "/temp/" : "temp/" + Guid.NewGuid().ToString() + "/";
            var container = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = container.GetBlobClient(path + file.FileName.ToLower());
            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = file.ContentType });
            return blobClient.Uri.ToString();
        }

        public string UrlDefaultImage()
        {
            return _blobServiceClient.GetBlobContainerClient(_containerName).Uri + "/temp/default.png";
        }
    }
}
