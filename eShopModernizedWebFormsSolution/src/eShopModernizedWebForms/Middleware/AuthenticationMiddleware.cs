using System.Security.Claims;

namespace eShopModernizedWebForms.Middleware
{
    /// <summary>
    /// A simple fallback authentication middleware that adds a basic identity
    /// when Azure Active Directory auth is not configured.
    /// </summary>
    public class AuthenticationMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var identity = new ClaimsIdentity("cookies");
            identity.AddClaim(new Claim("iat", "1234"));
            context.User = new ClaimsPrincipal(identity);
            await next(context);
        }
    }
}
