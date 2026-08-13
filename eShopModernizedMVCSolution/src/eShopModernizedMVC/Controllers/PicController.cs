using eShopModernizedMVC.Services;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace eShopModernizedMVC.Controllers
{
    public class PicController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(PicController));

        private readonly IImageService _imageService;

        public PicController(ICatalogService service, IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost]
        [Route("uploadimage")]
        public IActionResult UploadImage(IFormFile HelpSectionImages, [FromForm] string itemId)
        {
            _log.Info($"Now processing... /Pic/UploadImage");

            if (HelpSectionImages == null || HelpSectionImages.Length == 0)
            {
                return BadRequest("image is not valid");
            }

            int.TryParse(itemId, out var catalogItemId);
            var urlImageTemp = _imageService.UploadTempImage(HelpSectionImages, catalogItemId);
            var tempImage = new
            {
                name = new Uri(urlImageTemp).PathAndQuery,
                url = urlImageTemp
            };

            return Json(tempImage);
        }
    }
}
