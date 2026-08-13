using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace eShopModernizedMVC.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                var identity = new ClaimsIdentity("cookies");
                identity.AddClaim(new Claim("iat", "1234"));
                context.User = new ClaimsPrincipal(identity);
            }
            await _next(context);
        }
    }
}
