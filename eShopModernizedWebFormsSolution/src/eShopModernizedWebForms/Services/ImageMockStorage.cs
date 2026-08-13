using eShopModernizedWebForms.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace eShopModernizedWebForms.Services
{
    public class ImageMockStorage : IImageService
    {
        public string BaseUrl()
        {
            return GetBaseUrlImages();
        }

        public string BuildUrlImage(CatalogItem item)
        {
            var pictureFileName = string.IsNullOrEmpty(item.PictureFileName) ? "default.png" : item.PictureFileName;
            return GetBaseUrlImages() + pictureFileName;
        }

        public void Dispose()
        {
        }

        public void InitializeCatalogImages()
        {
        }

        public void UpdateImage(CatalogItem item)
        {
        }

        public string UploadTempImage(IFormFile file, int? catalogItemId)
        {
            if (!catalogItemId.HasValue)
                return UrlDefaultImage();

            return BaseUrl() + catalogItemId.Value + ".png";
        }

        public string UrlDefaultImage()
        {
            return GetBaseUrlImages() + "default.png";
        }

        private string GetBaseUrlImages()
        {
            return "/Pics/";
        }
    }
}
