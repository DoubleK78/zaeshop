using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Common.Models;
using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Domain.Interfaces.Infrastructure;
using Identity.Domain.Models.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Identity.Infrastructure.Implements.Infrastructure
{
    public class JwtService : IJwtService
    {
        private readonly AppIdentityDbContext _context;
        private readonly AppSettings _appSettings;
        private readonly UserManager<User> _userManager;

        public JwtService(
            AppIdentityDbContext context,
            IOptions<AppSettings> appSettings,
            UserManager<User> userManager)
        {
            _context = context;
            _appSettings = appSettings.Value;
            _userManager = userManager;
        }

        public string GenerateJwtToken(User user, int expirationInMinutes = 60)
        {
            var roles = _userManager.GetRolesAsync(user).Result;

            var claimsIdentity = new ClaimsIdentity();
            claimsIdentity.AddClaim(new Claim("id", user.Id));
            if (!string.IsNullOrEmpty(user.FullName))
                claimsIdentity.AddClaim(new Claim(ClaimTypes.GivenName, user.FullName));
            if (!string.IsNullOrEmpty(user.ProviderAccountId))
                claimsIdentity.AddClaim(new Claim("providerAccountId", user.ProviderAccountId));
            claimsIdentity.AddClaims(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                // Default CMS will only accept token in 15 minutes
                Expires = DateTime.UtcNow.AddMinutes(expirationInMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public UserInfomationTokenModel? ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            try
            {
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var userId = jwtToken.Claims.First(x => x.Type == "id").Value;
                var fullName = jwtToken.Claims.First(x => x.Type == "given_name").Value;
                var roles = jwtToken.Claims.Where(x => x.Type == "role").Select(x => x.Value).ToList();
                var providerAccountId = jwtToken.Claims.FirstOrDefault(x => x.Type == "providerAccountId")?.Value;

                // return user id from JWT token if validation successful
                var userInfomationTokenModel = new UserInfomationTokenModel
                {
                    Id = userId,
                    FullName = fullName,
                    Roles = roles,
                    ProviderAccountId = providerAccountId
                };
                return userInfomationTokenModel;
            }
            catch
            {
                // return null if validation fails
                return null;
            }
        }

        public UserToken GenerateRefreshToken(string ipAddress)
        {
            var refreshToken = new UserToken
            {
                Token = GetUniqueToken(),
                // token is valid for 7 days
                ExpiresOnUtc = DateTime.UtcNow.AddDays(7),
                CreatedByIp = ipAddress
            };

            return refreshToken;
        }

        private string GetUniqueToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            // ensure token is unique by checking against db
            var isExistsToken = _context.Users.Any(u => u.UserTokens.Any(t => t.Token == token));
            var tokenIsUnique = !isExistsToken;

            if (!tokenIsUnique)
                return GetUniqueToken();

            return token;
        }
    }
}