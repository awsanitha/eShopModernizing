using Microsoft.AspNetCore.Mvc;

namespace eShopLegacyMVC.Controllers.Api
{
    [Route("api")]
    public class CatalogController2 : ControllerBase
    {
        [HttpGet]
        public ActionResult Index()
        {
            return Ok(new { Message = "Hello World!" });
        }
    }
}
