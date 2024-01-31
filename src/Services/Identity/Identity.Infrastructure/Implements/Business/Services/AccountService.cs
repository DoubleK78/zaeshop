using System.Security.Cryptography;
using Common;
using Common.Enums;
using Common.Interfaces;
using Common.Interfaces.Messaging;
using Common.Models;
using Common.Shared.Models.Users;
using Identity.Domain.AggregatesModel.UserAggregate;
using Identity.Domain.Business.Interfaces.Services;
using Identity.Domain.Interfaces.Infrastructure;
using Identity.Domain.Interfaces.Messaging;
using Identity.Domain.Models.Authenticates;
using Identity.Domain.Models.ErrorCodes;
using Identity.Domain.Models.ErrorResponses;
using Identity.Domain.Models.HangFire;
using Identity.Domain.Models.Helpers;
using Identity.Domain.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Identity.Infrastructure.Implements.Business.Services
{
    public class AccountService : IAccountService
    {
        private readonly AppIdentityDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppSettings _appSettings;
        private readonly ISendMailPublisher _sendMailPublisher;
        private readonly IApiService _apiService;
        private readonly DbSet<UserToken> _userTokensDbSet;
        private readonly ISyncUserPortalPublisher _syncUserPortalPublisher;

        public AccountService(
            AppIdentityDbContext context,
            IJwtService jwtService,
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IOptions<AppSettings> appSettings,
            ISendMailPublisher sendMailPublisher,
            IApiService apiService,
            ISyncUserPortalPublisher syncUserPortalPublisher)
        {
            _context = context;
            _jwtService = jwtService;
            _userManager = userManager;
            _signInManager = signInManager;
            _appSettings = appSettings.Value;
            _sendMailPublisher = sendMailPublisher;
            _apiService = apiService;
            _userTokensDbSet = context.Set<UserToken>();
            _syncUserPortalPublisher = syncUserPortalPublisher;
        }

        #region Token
        public async Task<AuthenticateResponse?> AuthenticateAsync(AuthenticateRequest model, string ipAddress)
        {
            var user = await _context.Users.FirstOrDefaultAsync(o => o.UserName == model.Username);

            if (user == null)
                return null;

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded)
            {
                return new AuthenticateResponse
                {
                    ErrorResult = CommonHelper.GetDescription(ErrorCodes.UserOrPasswordNotCorrect)
                };
            }

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;
            _userTokensDbSet.Add(refreshToken);

            // Remove referesh token is not unnecessary
            RemoveOldRefreshTokens(user);

            // save changes to db
            await _context.SaveChangesAsync();

            return new AuthenticateResponse(user, jwtToken, refreshToken);
        }

        public async Task<AuthenticateResponse> RefreshTokenAsync(string token, string ipAddress)
        {
            var user = await GetUserByRefreshTokenAsync(token);
            var refreshToken = await _userTokensDbSet.FirstOrDefaultAsync(x => x.Token == token);

            if (user == null)
            {
                return new AuthenticateResponse
                {
                    ErrorResult = CommonHelper.GetDescription(ErrorCodes.UserNotExists)
                };
            }

            if (refreshToken?.IsActive != true)
            {
                return new AuthenticateResponse
                {
                    ErrorResult = CommonHelper.GetDescription(ErrorCodes.RefereshTokenNotActive)
                };
            }

            // replace old refresh token with a new one (rotate token)
            var newRefreshToken = RotateRefreshToken(refreshToken, ipAddress);
            newRefreshToken.UserId = user.Id;
            _userTokensDbSet.Add(newRefreshToken);
            _userTokensDbSet.Update(refreshToken);

            await _context.SaveChangesAsync();

            // generate new jwt
            var jwtToken = _jwtService.GenerateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken, newRefreshToken);
        }

        public async Task RevokeTokenAsync(string token, string ipAddress)
        {
            var user = await GetUserByRefreshTokenAsync(token);
            var refreshToken = user?.UserTokens.FirstOrDefault(x => x.Token == token);

            if (refreshToken?.IsActive != true)
            {
                return;
            }

            // revoke token and save
            RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
            _context.Update(user!);
            await _context.SaveChangesAsync();
        }

        private async Task<User?> GetUserByRefreshTokenAsync(string token)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.UserTokens.Any(t => t.Token == token));
            return user;
        }

        private UserToken RotateRefreshToken(UserToken refreshToken, string ipAddress)
        {
            var newRefreshToken = _jwtService.GenerateRefreshToken(ipAddress);
            RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
            return newRefreshToken;
        }

        private static void RevokeRefreshToken(UserToken token, string ipAddress, string? reason = null, string? replacedByToken = null)
        {
            token.RevokedOnUtc = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReasonRevoked = reason;
            token.ReplacedByToken = replacedByToken;
        }

        private void RemoveOldRefreshTokens(User user)
        {
            var expiredTokens = user.UserTokens.Where(token => !token.IsActive && token.CreatedOnUtc.AddDays(_appSettings.RefreshTokenTTL) <= DateTime.UtcNow).ToList();
            _userTokensDbSet.RemoveRange(expiredTokens);
        }
        #endregion

        #region Register/ Verify Email/ Resset Password
        public async Task<UserRegisterResponseModel?> RegisterAsync(UserRegisterRequestModel userModel, ErrorResult errorResult)
        {
            // Validate
            var existsUser = await _userManager.FindByNameAsync(userModel.Email);
            if (existsUser != null)
            {
                errorResult.Description = CommonHelper.GetDescription(ErrorCodes.UserExists);
                return null;
            }

            var user = new User
            {
                UserName = userModel.Username,
                Email = userModel.Email,
                FullName = userModel.FullName,
                VerificationToken = GenerateVerificationToken(),
                CreatedOnUtc = DateTime.UtcNow
            };

            // Create account
            var result = await _userManager.CreateAsync(user, userModel.Password);
            if (!result.Succeeded)
            {
                errorResult.Description = string.Join(", ", result.Errors.Select(o => o.Description));
                return null;
            }

            // Sync to portal
            var resultApi = await _apiService.PostAsync<SyncUserFromIdentityRequestModel, SyncUserFromIdentityResponseModel>(CommonHelper.GetServiceUrl(EServiceHost.Portal), "/v1/users", new SyncUserFromIdentityRequestModel
            {
                IdentityId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName
            });

            if (resultApi != null && !resultApi.IsSuccess)
            {
                await _userManager.DeleteAsync(user);
                errorResult.Description = resultApi.Message ?? string.Empty;
                return null;
            }

            // Send Email
            var userSendMailModel = new UserSendMailModel
            {
                FullName = user.FullName,
                Email = user.Email,
                VerificationToken = user.VerificationToken
            };
            await SendVerificationEmailAsync(userSendMailModel, "");

            return new UserRegisterResponseModel
            {
                Email = user.Email,
                FullName = user.FullName,
                UserName = user.UserName
            };
        }

        private string GenerateVerificationToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            // ensure token is unique by checking against db
            var tokenIsUnique = !_context.Users.Any(x => x.VerificationToken == token);
            if (!tokenIsUnique)
                return GenerateVerificationToken();

            return token;
        }

        public async Task<AuthenticateResponse?> VerifyEmailAsync(string token, string ipAddress, ErrorResult errorResult)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.VerificationToken == token);

            if (user == null)
            {
                errorResult.Description = CommonHelper.GetDescription(ErrorCodes.UserNotExists);
                return null;
            }

            user.VerificationToken = null;
            user.VerifiedOnUtc = DateTime.UtcNow;
            user.EmailConfirmed = true;

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);
            user.UserTokens.Add(refreshToken);

            _context.Users.Update(user);
            _context.SaveChanges();

            return new AuthenticateResponse(user, jwtToken, refreshToken);
        }

        public async Task ForgotPasswordAsync(ForgotPasswordRequest model, string origin, ErrorResult errorResult)
        {
            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == model.Email);

            // always return ok response to prevent email enumeration
            if (user == null)
            {
                errorResult.Description = CommonHelper.GetDescription(ErrorCodes.UserNotExists);
                return;
            }

            // create reset token that expires after 1 day
            user.ResetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            user.ResetPasswordTokenExpiresOnUtc = DateTime.UtcNow.AddDays(1);

            _context.Users.Update(user);
            _context.SaveChanges();

            // Send Email
            var userSendMailModel = new UserSendMailModel
            {
                FullName = user.FullName,
                Email = user.Email,
                ResetPasswordToken = user.ResetPasswordToken
            };
            await SendPasswordResetEmailAsync(userSendMailModel, "");
        }

        private string GenerateResetToken()
        {
            // token is a cryptographically strong random sequence of values
            var token = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));

            // ensure token is unique by checking against db
            var tokenIsUnique = !_context.Users.Any(x => x.ResetPasswordToken == token);
            if (!tokenIsUnique)
                return GenerateResetToken();

            return token;
        }

        private async Task<User?> GetAccountByResetTokenAsync(string token)
        {
            var account = await _context.Users.FirstOrDefaultAsync(x =>
                x.ResetPasswordToken == token && x.ResetPasswordTokenExpiresOnUtc > DateTime.UtcNow);
            return account;
        }

        public async Task ValidateResetTokenAsync(ValidateResetTokenRequest model, ErrorResult errorResult)
        {
            var user = await GetAccountByResetTokenAsync(model.Token);
            if (user == null)
            {
                errorResult.Description = CommonHelper.GetDescription(ErrorCodes.UserNotExists);
            }
        }

        public async Task<AuthenticateResponse?> ResetPasswordAsync(ResetPasswordRequest model, string ipAddress, ErrorResult errorResult)
        {
            var user = await GetAccountByResetTokenAsync(model.Token);
            if (user == null)
            {
                return null;
            }

            // update password and remove reset token
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
            {
                errorResult.Description = string.Join(", ", result.Errors.Select(o => o.Description));
                return null;
            }

            user.ResetPasswordOnUtc = DateTime.UtcNow;

            _context.Users.Update(user);
            _context.SaveChanges();

            // authentication successful so generate jwt and refresh tokens
            var jwtToken = _jwtService.GenerateJwtToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);
            refreshToken.UserId = user.Id;
            _userTokensDbSet.Add(refreshToken);

            // Remove referesh token is not unnecessary
            RemoveOldRefreshTokens(user);

            // save changes to db
            await _context.SaveChangesAsync();

            return new AuthenticateResponse(user, jwtToken, refreshToken);
        }
        #endregion

        #region Email
        private async Task SendVerificationEmailAsync(UserSendMailModel userModel, string origin)
        {
            string message;

            if (!string.IsNullOrEmpty(origin))
            {
                // origin exists if request sent from browser single page app (e.g. Angular or React)
                // so send link to verify via single page app
                var verifyUrl = $"{origin}/account/verify-email?token={userModel.VerificationToken}";
                message = $@"<p>Please click the below link to verify your email address:</p>
                            <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
            }
            else
            {
                // origin missing if request sent directly to api (e.g. from Postman)
                // so send instructions to verify directly with api
                message = $@"<p>Please use the below token to verify your email address with the <code>/accounts/verify-email</code> api route:</p>
                            <p><code>{userModel.VerificationToken}</code></p>";
            }

            string subject = "Sign-up Verification API - Verify Email";
            string body = $@"
                        <h4>Hi <b>{userModel.FullName}</b>!</h4>
                        <h3>Verify Email</h3>
                        <p>Thanks for registering!</p>
                        {message}";

            await _sendMailPublisher.SendMailAsync(new Common.Shared.Models.Emails.SendEmailMessage
            {
                Subject = subject,
                Body = body,
                ToEmails = new List<string> { userModel.Email ?? string.Empty }
            });
        }

        private async Task SendPasswordResetEmailAsync(UserSendMailModel userModel, string origin)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/account/reset-password?token={userModel.ResetPasswordToken}";
                message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                            <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                            <p><code>{userModel.ResetPasswordToken}</code></p>";
            }

            string subject = "Sign-up Verification API - Reset Password";
            string body = $@"
                        <h4>Hi <b>{userModel.FullName}</b>!</h4>
                        <h4>Reset Password Email</h4>
                        {message}";

            await _sendMailPublisher.SendMailAsync(new Common.Shared.Models.Emails.SendEmailMessage
            {
                Subject = subject,
                Body = body,
                ToEmails = new List<string> { userModel.Email ?? string.Empty }
            });
        }
        #endregion

        public async Task<ServiceResponse<AuthenticateResponse>> ClientAuthenticateAsync(ClientAuthenticateRequest model)
        {
            // check providerAccountId and email in database
            // if not then create a new user
            var user = await _context.Users.FirstOrDefaultAsync(o => o.ProviderAccountId == model.ProviderAccountId || o.Email == model.Email);
            if (user == null)
            {
                user = new User
                {
                    UserName = model.Email.Split('@').FirstOrDefault(),
                    Email = model.Email,
                    FullName = model.Name.Split(' ').FirstOrDefault() ?? string.Empty,
                    ProviderAccountId = model.ProviderAccountId,
                    // Flag to check Client registered
                    IsClientRegistered = true,
                    CreatedOnUtc = DateTime.UtcNow,
                    Avatar = model.Image
                };

                // Create account
                var result = await _userManager.CreateAsync(user);
                if (!result.Succeeded)
                {
                    return new ServiceResponse<AuthenticateResponse>(string.Join(", ", result.Errors.Select(o => o.Description)));
                }

                // Sync to portal
                var resultApi = await _apiService.PostAsync<SyncUserFromIdentityRequestModel, SyncUserFromIdentityResponseModel>(CommonHelper.GetServiceUrl(EServiceHost.Portal), "/v1/users", new SyncUserFromIdentityRequestModel
                {
                    IdentityId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    UserName = user.UserName,
                    Avatar = user.Avatar
                });

                if (resultApi != null && !resultApi.IsSuccess)
                {
                    await _userManager.DeleteAsync(user);
                    return new ServiceResponse<AuthenticateResponse>(resultApi.Message ?? string.Empty);
                }
            }
            else if (string.IsNullOrEmpty(user.ProviderAccountId))
            {
                user.ProviderAccountId = model.ProviderAccountId;
                user.UpdatedOnUtc = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
            }
            else if (user.FullName != model.Name.Split(' ').FirstOrDefault() || user.Avatar != model.Image)
            {
                user.FullName = model.Name.Split(' ').FirstOrDefault() ?? string.Empty;
                user.Avatar = model.Image;
                user.UpdatedOnUtc = DateTime.UtcNow;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Sync portal with message queue
                await _syncUserPortalPublisher.SyncUserPortalAsync(new SyncUserPortalMessage
                {
                    IdentityUserId = user.Id,
                    FullName = user.FullName,
                    Avatar = user.Avatar,
                    IsUpdateAvatar = true
                });
            }

            // Token expries in 30 days
            var expirationInMinutes = 60 * 24 * 30;
            var jwtToken = _jwtService.GenerateJwtToken(user, expirationInMinutes);
            return new ServiceResponse<AuthenticateResponse>(new AuthenticateResponse(user, jwtToken));
        }
    }
}