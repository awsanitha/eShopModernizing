using eShopLegacyMVC.Services;
using log4net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;

namespace eShopLegacyMVC.Controllers
{
    public class PicController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(PicController));

        public const string GetPicRouteName = "GetPicRouteTemplate";

        private readonly ICatalogService service;
        private readonly IWebHostEnvironment webHostEnvironment;

        public PicController(ICatalogService service, IWebHostEnvironment webHostEnvironment)
        {
            this.service = service;
            this.webHostEnvironment = webHostEnvironment;
        }

        // GET: Pic/5.png
        [HttpGet]
        [Route("items/{catalogItemId:int}/pic", Name = GetPicRouteName)]
        public ActionResult Index(int catalogItemId)
        {
            _log.Info($"Now loading... /items/Index?{catalogItemId}/pic");

            if (catalogItemId <= 0)
            {
                return BadRequest();
            }

            var item = service.FindCatalogItem(catalogItemId);

            if (item != null)
            {
                var picsPath = Path.Combine(webHostEnvironment.WebRootPath ?? webHostEnvironment.ContentRootPath, "Pics");
                var path = Path.Combine(picsPath, item.PictureFileName ?? "dummy.png");

                if (!System.IO.File.Exists(path))
                {
                    return NotFound();
                }

                string imageFileExtension = Path.GetExtension(item.PictureFileName) ?? string.Empty;
                string mimetype = GetImageMimeTypeFromImageFileExtension(imageFileExtension);

                var buffer = System.IO.File.ReadAllBytes(path);
                return File(buffer, mimetype);
            }

            return NotFound();
        }

        private static string GetImageMimeTypeFromImageFileExtension(string? extension)
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
