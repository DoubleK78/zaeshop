using System.Net;
using Grpc.Core;
using Portal.API.Extensions;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Enums;
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

            try
            {
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
                    Avatar = request.Avatar,
                    Region = request.Region?.ToLower() == "vi" ? ERegion.vi : ERegion.en
                };

                _unitOfWork.Repository<User>().Add(user);
                await _unitOfWork.SaveChangesAsync();

                // Check data is integrity
                var parameters = new Dictionary<string, object?>
                {
                    { "id",  user.Id },
                    { "identityUserId", user.IdentityUserId }
                };

                var isSuccess = (await _unitOfWork.QueryAsync<bool?>("SELECT 1 from [User] WHERE Id = @id OR IdentityUserId = @identityUserId", parameters, commandType: CommandType.Text, commandTimeout: 90)).FirstOrDefault();
                if (!isSuccess.HasValue || !isSuccess.Value)
                {
                    return new SyncUserReply
                    {
                        PortalId = 0,
                        IsSuccess = false,
                        Message = HttpStatusCode.BadRequest.ToString()
                    };
                }

                return new SyncUserReply
                {
                    IsSuccess = true,
                    Message = HttpStatusCode.OK.ToString(),
                    PortalId = user.Id
                };
            }
            catch (Exception)
            {
                // Before we throw exception, then we should return client knows.
                return new SyncUserReply
                {
                    PortalId = 0,
                    IsSuccess = false,
                    Message = HttpStatusCode.InternalServerError.ToString()
                };
                throw;
            }
        }
    }
}
