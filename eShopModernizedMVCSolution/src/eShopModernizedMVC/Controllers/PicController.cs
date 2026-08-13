using eShopModernizedMVC.Services;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace eShopModernizedMVC.Controllers
{
    public class PicController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IImageService _imageService;

        public PicController(ICatalogService service, IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost]
        [Route("uploadimage")]
        public async Task<IActionResult> UploadImage(IFormFile image, string itemId)
        {
            _log.Info($"Now processing... /Pic/UploadImage");

            if (image == null || image.Length == 0)
                return BadRequest("image is not valid");

            int.TryParse(itemId, out var catalogItemId);
            var urlImageTemp = await _imageService.UploadTempImageAsync(image, catalogItemId > 0 ? catalogItemId : (int?)null);

            var tempImage = new
            {
                name = new Uri(urlImageTemp).PathAndQuery,
                url = urlImageTemp
            };

            return Json(tempImage);
        }
    }
}
