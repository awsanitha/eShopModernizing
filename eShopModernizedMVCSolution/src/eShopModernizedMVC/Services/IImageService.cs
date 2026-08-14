using eShopModernizedMVC.Models;
using Microsoft.AspNetCore.Http;

namespace eShopModernizedMVC.Services
{
    public interface IImageService
    {
        string UploadTempImage(IFormFile file, int? catalogItemId);
        string BaseUrl();
        void UpdateImage(CatalogItem item);
        string UrlDefaultImage();
        string BuildUrlImage(CatalogItem item);
        void InitializeCatalogImages();
    }
}
