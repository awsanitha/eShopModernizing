using System;
using eShopModernizedMVC.Services;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace eShopModernizedMVC.Controllers
{
    public class PicController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly string[] ValidContentTypes = { "image/jpeg", "image/png", "image/gif" };
        private readonly IImageService _imageService;

        public PicController(ICatalogService service, IImageService imageService)
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

            if (!Array.Exists(ValidContentTypes, ct => string.Equals(ct, file.ContentType, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var header = new byte[8];
                    int bytesRead = stream.Read(header, 0, header.Length);

                    if (bytesRead >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF)
                    {
                        return true; // JPEG
                    }
                    if (bytesRead >= 8 && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47)
                    {
                        return true; // PNG
                    }
                    if (bytesRead >= 3 && header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46)
                    {
                        return true; // GIF
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }
    }
}
