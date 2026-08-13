using eShopPorted.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace eShopPorted.Controllers
{
    public class PicController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string GetPicRouteName = "GetPicRouteTemplate";
        private readonly ICatalogService service;

        public PicController(ICatalogService service)
        {
            this.service = service;
        }

        // GET: Pic/5.png
        [HttpGet]
        [Route("items/{catalogItemId:int}/pic", Name = GetPicRouteName)]
        public IActionResult Index(int catalogItemId)
        {
            _log.Info($"Now loading... /items/{catalogItemId}/pic");
            if (catalogItemId <= 0) return BadRequest();

            var item = service.FindCatalogItem(catalogItemId);
            if (item != null)
            {
                var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Pics");
                var path = Path.Combine(webRoot, item.PictureFileName);
                string mimeType = GetImageMimeTypeFromImageFileExtension(Path.GetExtension(item.PictureFileName));
                var buffer = System.IO.File.ReadAllBytes(path);
                return File(buffer, mimeType);
            }

            return NotFound();
        }

        private static string GetImageMimeTypeFromImageFileExtension(string extension)
        {
            return extension?.ToLower() switch
            {
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".bmp" => "image/bmp",
                ".tiff" => "image/tiff",
                ".svg" => "image/svg+xml",
                _ => "application/octet-stream"
            };
        }
    }
}
