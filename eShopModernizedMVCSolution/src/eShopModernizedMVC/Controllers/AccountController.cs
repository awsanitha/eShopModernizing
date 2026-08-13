using log4net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace eShopModernizedMVC.Controllers
{
    public class AccountController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(AccountController));

        public IActionResult SignIn()
        {
            _log.Info($"Now processing... AccountController.SignIn");
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                return Challenge(new AuthenticationProperties { RedirectUri = "/" }, OpenIdConnectDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Index", "Catalog");
        }

        public IActionResult SignOut()
        {
            _log.Info($"Now processing... AccountController.SignOut");
            return SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public IActionResult EndSession()
        {
            _log.Info($"Now processing... AccountController.EndSession");
            return SignOut(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
