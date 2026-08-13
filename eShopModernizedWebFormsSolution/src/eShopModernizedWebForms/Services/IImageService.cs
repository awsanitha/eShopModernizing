using eShopModernizedWebForms.Models;

namespace eShopModernizedWebForms.Services
{
    public interface IImageService : IDisposable
    {
        string UploadTempImage(IFormFile file, int? catalogItemId);
        string BaseUrl();
        void UpdateImage(CatalogItem item);
        string UrlDefaultImage();
        string BuildUrlImage(CatalogItem item);
        void InitializeCatalogImages();
    }
}
