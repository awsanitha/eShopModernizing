using Microsoft.AspNetCore.Mvc;

namespace eShopLegacyWebForms.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        [Route("About")]
        public IActionResult About()
        {
            return View();
        }

        [HttpGet]
        [Route("Contact")]
        public IActionResult Contact()
        {
            return View();
        }
    }
}
