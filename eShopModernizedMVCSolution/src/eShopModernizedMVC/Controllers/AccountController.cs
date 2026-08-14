using log4net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace eShopModernizedMVC.Controllers
{
    public class AccountController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public async Task<IActionResult> SignIn()
        {
            _log.Info($"Now processing... AccountController.SignIn");
            // Send an OpenID Connect sign-in request.
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                await HttpContext.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
                return new EmptyResult();
            }
            return RedirectToAction("Index", "Catalog");
        }

        public new async Task<IActionResult> SignOut()
        {
            _log.Info($"Now processing... AccountController.SignOut");
            // Send an OpenID Connect sign-out request.
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return new EmptyResult();
        }

        public async Task<IActionResult> EndSession()
        {
            _log.Info($"Now processing... AccountController.EndSession");
            // If AAD sends a single sign-out message to the app, end the user's session, but don't redirect to AAD for sign out.
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return new EmptyResult();
        }
    }
}
