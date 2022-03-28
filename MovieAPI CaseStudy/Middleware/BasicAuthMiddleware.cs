using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MovieAPI_CaseStudy.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class BasicAuthMiddleware
    {
        private readonly RequestDelegate _next;
        private static IConfiguration _config;

        public BasicAuthMiddleware(RequestDelegate next, IConfiguration config)
        {
            _next = next;
            _config = config;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            string authHeader = httpContext.Request.Headers["Authorization"];
            try
            {
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring(6).Trim();
                    var credentialString = Encoding.UTF8.GetString(Convert.FromBase64String(token));
                    var credentials = credentialString.Split(':');

                    if (credentials[0] == _config.GetSection("BasicAuthCredentials:Username").Value
                        && credentials[1] == _config.GetSection("BasicAuthCredentials:Password").Value)
                    {
                        var claims = new[]
                        {
                            new Claim("name", credentials[0]),
                            new Claim(ClaimTypes.Role, "Admin")
                        };

                        var identity = new ClaimsIdentity(claims, "Basic");
                        httpContext.User = new ClaimsPrincipal(identity);

                        await _next(httpContext);
                    }
                    else
                    {
                        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    }
                }
                else
                {
                    httpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
                }
            }
            catch
            {
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }

            return;
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class AuthenticationMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuthenticationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BasicAuthMiddleware>();
        }
    }
}
