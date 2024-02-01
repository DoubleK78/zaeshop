using System.Net;
using Grpc.Core;
using Portal.API.Extensions;
using Portal.Domain.AggregatesModel.UserAggregate;
using PortalGrpc;

namespace Portal.API.Controllers
{
    public class UserGrpcController : UserGrpcService.UserGrpcServiceBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserGrpcController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public override async Task<UsersReply> GetUsers(UserRequest request, ServerCallContext context)
        {
            if (!context.IsAllowedHost())
            {
                return new UsersReply();
            }

            var users = await _unitOfWork.Repository<User>().GetQueryable()
                .Project(x => new User { Id = x.Id, FullName = x.FullName, IdentityUserId = x.IdentityUserId })
                .ToListAsync();
            var usersReply = new UsersReply();

            foreach (var user in users)
            {
                usersReply.UserReply.Add(new UserReply
                {
                    Id = user.Id.ToString(),
                    FullName = user.FullName ?? string.Empty,
                    IdentityId = user.IdentityUserId
                });
            }

            return usersReply;
        }

        public override async Task<SyncUserReply> SyncUserFromIdentity(SyncUserRequest request, ServerCallContext context)
        {
            if (!context.IsAllowedHost())
            {
                return new SyncUserReply
                {
                    PortalId = 0,
                    IsSuccess = false,
                    Message = "Unauthorized"
                };
            }

            // Validate model
            if (string.IsNullOrEmpty(request.IdentityId))
            {
                return new SyncUserReply
                {
                    PortalId = 0,
                    IsSuccess = false,
                    Message = "Identity Id is required."
                };
            }

            if (string.IsNullOrEmpty(request.FullName))
            {
                return new SyncUserReply
                {
                    PortalId = 0,
                    IsSuccess = false,
                    Message = "FullName is required."
                };
            }

            if (string.IsNullOrEmpty(request.Email))
            {
                return new SyncUserReply
                {
                    PortalId = 0,
                    IsSuccess = false,
                    Message = "Email is required."
                };
            }

            if (string.IsNullOrEmpty(request.UserName))
            {
                return new SyncUserReply
                {
                    PortalId = 0,
                    IsSuccess = false,
                    Message = "UserName is required."
                };
            }

            var hasUserExists = await _unitOfWork.Repository<User>().GetQueryable().AnyAsync(x =>
                x.IdentityUserId == request.IdentityId ||
                x.Email == request.Email ||
                x.UserName == request.UserName);
            if (hasUserExists)
            {
                return new SyncUserReply
                {
                    PortalId = 0,
                    IsSuccess = false,
                    Message = HttpStatusCode.BadRequest.ToString()
                };
            }

            var user = new User
            {
                IdentityUserId = request.IdentityId,
                FullName = request.FullName,
                Email = request.Email,
                UserName = request.UserName,
                Avatar = request.Avatar
            };

            _unitOfWork.Repository<User>().Add(user);
            await _unitOfWork.SaveChangesAsync();

            return new SyncUserReply
            {
                IsSuccess = true,
                Message = HttpStatusCode.OK.ToString(),
                PortalId = user.Id
            };
        }
    }
}
