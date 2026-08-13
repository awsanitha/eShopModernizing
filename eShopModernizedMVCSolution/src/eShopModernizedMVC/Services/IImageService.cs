using eShopModernizedMVC.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace eShopModernizedMVC.Services
{
    public interface IImageService : IDisposable
    {
        Task<string> UploadTempImageAsync(IFormFile file, int? catalogItemId);
        string BaseUrl();
        void UpdateImage(CatalogItem item);
        string UrlDefaultImage();
        string BuildUrlImage(CatalogItem item);
        void InitializeCatalogImages();
    }
}
