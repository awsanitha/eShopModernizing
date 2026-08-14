using System;
using System.Linq;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using eShopModernizedWebForms.Services;

namespace eShopModernizedWebForms.Controllers
{
    public class PicController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string[] ValidContentTypes = { "image/jpeg", "image/png", "image/gif" };

        private readonly IImageService _imageService;

        public PicController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost]
        [Route("uploadimage")]
        public ActionResult UploadImage()
        {
            _log.Info($"Now processing... /Pic/UploadImage");
            IFormFile image = Request.Form.Files["HelpSectionImages"];
            var itemId = Request.Form["itemId"];

            if (!IsValidImage(image))
            {
                return BadRequest("image is not valid");
            }

            int.TryParse(itemId, out var catalogItemId);
            var urlImageTemp = _imageService.UploadTempImage(image, catalogItemId);
            var tempImage = new
            {
                name = new Uri(urlImageTemp).PathAndQuery,
                url = urlImageTemp
            };

            return Json(tempImage);
        }

        private bool IsValidImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return false;
            }

            return ValidContentTypes.Contains(file.ContentType?.ToLowerInvariant());
        }
    }
}
