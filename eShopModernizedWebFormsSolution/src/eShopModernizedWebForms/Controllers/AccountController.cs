using log4net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace eShopModernizedWebForms.Controllers
{
    public class AccountController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AccountController));

        public IActionResult SignIn()
        {
            _log.Info("Now processing... AccountController.SignIn");
            if (!User.Identity!.IsAuthenticated)
            {
                return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Index", "Catalog");
        }

        public new async Task<IActionResult> SignOut()
        {
            _log.Info("Now processing... AccountController.SignOut");
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Catalog");
        }

        public async Task<IActionResult> EndSession()
        {
            _log.Info("Now processing... AccountController.EndSession");
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Catalog");
        }
    }
}
