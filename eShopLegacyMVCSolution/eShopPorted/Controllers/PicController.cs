using eShopPorted.Services;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace eShopPorted.Controllers
{
    public class PicController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType);

        public const string GetPicRouteName = "GetPicRouteTemplate";

        private readonly ICatalogService _service;
        private readonly IWebHostEnvironment _env;

        public PicController(ICatalogService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // GET: Pic/5.png
        [HttpGet]
        [Route("items/{catalogItemId:int}/pic", Name = GetPicRouteName)]
        public IActionResult Index(int catalogItemId)
        {
            _log.Info($"Now loading... /items/Index?{catalogItemId}/pic");

            if (catalogItemId <= 0)
            {
                return BadRequest();
            }

            var item = _service.FindCatalogItem(catalogItemId);

            if (item != null)
            {
                var webRoot = Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, "Pics");
                var path = Path.Combine(webRoot, item.PictureFileName ?? "dummy.png");

                string imageFileExtension = Path.GetExtension(item.PictureFileName ?? ".png");
                string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

                var buffer = System.IO.File.ReadAllBytes(path);

                return File(buffer, mimetype);
            }

            return NotFound();
        }

        private static string GetImageMimeTypeFromImageFileExtension(string extension)
        {
            return extension switch
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
