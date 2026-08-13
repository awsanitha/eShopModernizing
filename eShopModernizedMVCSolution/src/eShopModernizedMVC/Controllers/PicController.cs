using eShopModernizedMVC.Services;
using log4net;
using Microsoft.AspNetCore.Mvc;

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
        public IActionResult UploadImage([FromForm] IFormFile? HelpSectionImages, [FromForm] string? itemId)
        {
            _log.Info("Now processing... /Pic/UploadImage");

            if (HelpSectionImages == null || HelpSectionImages.Length == 0)
                return BadRequest("image is not valid");

            if (!IsValidImage(HelpSectionImages))
                return BadRequest("image is not valid");

            int.TryParse(itemId, out var catalogItemId);
            var urlImageTemp = _imageService.UploadTempImage(HelpSectionImages, catalogItemId > 0 ? catalogItemId : (int?)null);
            var tempImage = new
            {
                name = new Uri(urlImageTemp).PathAndQuery,
                url = urlImageTemp
            };

            return Json(tempImage);
        }

        private static bool IsValidImage(IFormFile file)
        {
            // Check by extension and content type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(file.FileName)?.ToLowerInvariant();
            return allowedExtensions.Contains(ext);
        }
    }
}
