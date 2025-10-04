using ASP_421.Data.Entities;
using System.Security.Claims;
using System.Text.Json;

namespace ASP_421.Middleware
{
    public class AuthSessionMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthSessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Query.ContainsKey("logout"))
            {
                context.Session.Remove("SignIn");
                context.Response.Redirect(context.Request.Path);
                return;
            }

            if (context.Session.Keys.Contains("SignIn"))
            {
                UserAccess userAccess =
                    JsonSerializer.Deserialize<UserAccess>(
                        context.Session.GetString("SignIn")!)!;

                context.User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        [
                            new Claim(ClaimTypes.Name, userAccess.User.Name),
                            new Claim(ClaimTypes.Email, userAccess.User.Email),
                            new Claim(ClaimTypes.PrimarySid, userAccess.User.Id.ToString()),
                            new Claim(ClaimTypes.NameIdentifier, userAccess.Login),
                            new Claim(ClaimTypes.Role, userAccess.Role.Id)
                        ],
                        nameof(AuthSessionMiddleware)
                    )
                );
            }
            await _next(context);
        }
    }

    public static class AuthSessionMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthSession(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthSessionMiddleware>();
        }
    }

}
