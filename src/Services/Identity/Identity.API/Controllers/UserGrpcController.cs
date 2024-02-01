using Grpc.Core;
using Identity.API.Extensions;
using Identity.Domain.Business.Interfaces.Services;
using IdentityGrpc;

namespace Identity.API.Controllers;

public class UserGrpcController : UserGrpcService.UserGrpcServiceBase
{
    #region fields
    private readonly IUserService _userService;
    #endregion

    #region ctor
    public UserGrpcController(IUserService userService)
    {
        _userService = userService;
    }
    #endregion

    public override async Task<UsersReply> GetUsers(UserRequest request, ServerCallContext context)
    {
        if (!context.IsAllowedHost())
        {
            return new UsersReply();
        }

        var users = await _userService.GetAllAsync();
        var usersReply = new UsersReply();

        foreach (var user in users)
        {
            usersReply.UserReply.Add(new UserReply
            {
                FullName = user.FullName,
                ResetPasswordToken = user.ResetPasswordToken ?? string.Empty,
                VerificationToken = user.VerificationToken ?? string.Empty
            });
        }

        return usersReply;
    }
}