using System.Text.Json.Serialization;
using Identity.Domain.AggregatesModel.UserAggregate;

namespace Identity.Domain.Models.Authenticates
{
    public class AuthenticateResponse
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? JwtToken { get; set; }

        [JsonIgnore] // refresh token is returned in http only cookie
        public string? RefreshToken { get; set; }

        [JsonIgnore]
        public string? ErrorResult { get; set; }

        [JsonIgnore]
        public DateTime? ExpiresOnUtc { get; set; }

        public AuthenticateResponse()
        {
        }

        public AuthenticateResponse(User user, string? jwtToken, UserToken userToken)
        {
            Id = user.Id;
            FullName = user.FullName;
            UserName = user.UserName;
            JwtToken = jwtToken;
            RefreshToken = userToken?.Token;
            ExpiresOnUtc = userToken?.ExpiresOnUtc;
        }

        public AuthenticateResponse(User user, string? jwtToken)
        {
            Id = user.Id;
            FullName = user.FullName;
            UserName = user.UserName;
            JwtToken = jwtToken;
        }
    }

    public class AuthenticateWithRolesResponse : AuthenticateResponse
    {
        public List<string> Roles { get; set; } = null!;

        public AuthenticateWithRolesResponse(User user, string? jwtToken, UserToken userToken, List<string> roles) : base(user, jwtToken, userToken)
        {
            Roles = roles;
        }

        public AuthenticateWithRolesResponse(User user, string? jwtToken, List<string> roles) : base(user, jwtToken)
        {
            Roles = roles;
        }

        public AuthenticateWithRolesResponse()
        {
        }
    }
}