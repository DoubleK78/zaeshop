using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace Portal.API.Controllers
{
    public class BaseApiController : ControllerBase
    {
        [NonAction]
        public string? GetIdentityUserIdByToken()
        {
            var identityUserId = User.FindFirstValue("id");
            return identityUserId;
        }

        [NonAction]
        public string? IpAddress()
        {
            var ipAddress = GetIpAddress();
            return ipAddress?.Split(',').FirstOrDefault();
        }

        private string? GetIpAddress()
        {
            // get source ip address for the current request
            if (Request.Headers.TryGetValue("X-Forwarded-For", out Microsoft.Extensions.Primitives.StringValues value))
                return value;
            else
                return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString();
        }
    }
}
