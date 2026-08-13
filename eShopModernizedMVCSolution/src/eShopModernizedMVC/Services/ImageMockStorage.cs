using eShopModernizedMVC.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace eShopModernizedMVC.Services
{
    public class ImageMockStorage : IImageService
    {
        public string BaseUrl() => GetBaseUrlImages();

        public string BuildUrlImage(CatalogItem item)
        {
            var pictureFileName = string.IsNullOrEmpty(item.PictureFileName) ? "default.png" : item.PictureFileName;
            return GetBaseUrlImages() + pictureFileName;
        }

        public void Dispose() { }

        public void InitializeCatalogImages() { }

        public void UpdateImage(CatalogItem item) { }

        public Task<string> UploadTempImageAsync(IFormFile file, int? catalogItemId)
        {
            if (!catalogItemId.HasValue)
                return Task.FromResult(UrlDefaultImage());
            return Task.FromResult(BaseUrl() + catalogItemId.Value + ".png");
        }

        public string UrlDefaultImage() => GetBaseUrlImages() + "default.png";

        private string GetBaseUrlImages() => "/Pics/";
    }
}
