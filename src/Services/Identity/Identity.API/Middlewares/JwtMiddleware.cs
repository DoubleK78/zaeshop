using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Identity.Infrastructure.Interfaces.Services;
using Identity.Infrastructure.Models.Helpers;
using Microsoft.Extensions.Options;

namespace Identity.API.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly AppSettings _appSettings;

        public JwtMiddleware(
            RequestDelegate next,
            IOptions<AppSettings> appSettings)
        {
            _next = next;
            _appSettings = appSettings.Value;
        }

        public async Task Invoke(HttpContext context, IUserService userService, IJwtService jwtService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split("Bearer ").Last();
            var userId = jwtService.ValidateJwtToken(token ?? string.Empty);

            if (!string.IsNullOrEmpty(userId))
            {
                // attach user to context on successful jwt validation
                context.Items["User"] = await userService.GetByIdAsync(userId);
            }

            await _next(context);
        }
    }
}