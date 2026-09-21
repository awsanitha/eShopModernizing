using eShopLegacyMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace eShopLegacyMVC.Controllers
{
    public class PicController : Controller
    {
        public const string GetPicRouteName = "GetPicRouteTemplate";

        private readonly ICatalogService _service;
        private readonly IWebHostEnvironment _env;

        public PicController(ICatalogService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        [HttpGet("items/{catalogItemId:int}/pic", Name = GetPicRouteName)]
        public IActionResult GetImage(int catalogItemId)
        {
            var item = _service.FindCatalogItem(catalogItemId);
            if (item == null) return NotFound();

            var picFilePath = Path.Combine(_env.ContentRootPath, "Pics", item.PictureFileName);
            if (!System.IO.File.Exists(picFilePath))
            {
                picFilePath = Path.Combine(_env.ContentRootPath, "Pics", "dummy.png");
            }

            if (!System.IO.File.Exists(picFilePath))
                return NotFound();

            var buffer = System.IO.File.ReadAllBytes(picFilePath);
            var mimetype = GetImageMimeTypeFromImageFileExtension(
                Path.GetExtension(item.PictureFileName));
            return File(buffer, mimetype);
        }

        private static string GetImageMimeTypeFromImageFileExtension(string? extension)
        {
            return extension?.ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                ".wmf" => "image/wmf",
                ".jp2" => "image/jp2",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}
