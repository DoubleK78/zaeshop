using Common.Models;
using Identity.Domain.Models.Authenticates;
using Identity.Domain.Models.ErrorResponses;
using Identity.Domain.Models.Users;

namespace Identity.Domain.Business.Interfaces.Services
{
    public interface IAccountService
    {
        Task<AuthenticateWithRolesResponse?> AuthenticateAsync(AuthenticateRequest model, string ipAddress);
        Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress);
        Task RevokeTokenAsync(string token, string ipAddress);
        Task<UserRegisterResponseModel?> RegisterAsync(UserRegisterRequestModel userModel, ErrorResult errorResult);
        Task<AuthenticateResponse?> VerifyEmailAsync(string token, string ipAddress, ErrorResult errorResult);
        Task ValidateResetTokenAsync(ValidateResetTokenRequest model, ErrorResult errorResult);
        Task ForgotPasswordAsync(ForgotPasswordRequest model, string origin, ErrorResult errorResult);
        Task<AuthenticateResponse?> ResetPasswordAsync(ResetPasswordRequest model, string ipAddress, ErrorResult errorResult);
        Task<ServiceResponse<AuthenticateWithRolesResponse>> ClientAuthenticateAsync(ClientAuthenticateRequest model);
    }
}