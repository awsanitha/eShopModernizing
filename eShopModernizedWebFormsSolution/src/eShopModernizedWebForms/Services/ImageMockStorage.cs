using eShopModernizedWebForms.Models;

namespace eShopModernizedWebForms.Services
{
    public class ImageMockStorage : IImageService
    {
        private readonly IWebHostEnvironment _env;

        public ImageMockStorage(IWebHostEnvironment env)
        {
            _env = env;
        }

        public string BaseUrl() => GetBaseUrlImages();

        public string BuildUrlImage(CatalogItem item)
        {
            var pictureFileName = string.IsNullOrEmpty(item.PictureFileName) ? "default.png" : item.PictureFileName;
            return GetBaseUrlImages() + pictureFileName;
        }

        public void Dispose() { }

        public void InitializeCatalogImages() { }

        public void UpdateImage(CatalogItem item) { }

        public string UploadTempImage(IFormFile file, int? catalogItemId)
        {
            if (!catalogItemId.HasValue)
                return UrlDefaultImage();

            var picsPath = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "Pics");
            var imageExists = File.Exists(Path.Combine(picsPath, catalogItemId.Value + ".png"));

            if (imageExists)
                return BaseUrl() + catalogItemId.Value + ".png";

            return UrlDefaultImage();
        }

        public string UrlDefaultImage() => GetBaseUrlImages() + "default.png";

        private static string GetBaseUrlImages() => "/Pics/";
    }
}
