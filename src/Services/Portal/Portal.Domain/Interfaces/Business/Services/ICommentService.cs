using Common.Models;
using Portal.Domain.Models.CommentModels;

namespace Portal.Domain.Interfaces.Business.Services
{
    public interface ICommentService
    {
        Task<ServiceResponse<CommentModel>> CreateAsync(CommentRequestModel request, string identityUserId);
        Task<ServiceResponse<CommentModel>> UpdateAsync(int id, CommentRequestModel request, string identityUserId);
        Task<ServiceResponse<bool>> DeleteAsync(int id, string identityUserId);
        Task<ServiceResponse<PagingCommonResponse<CommentPagingResposneModel>>> GetPagingAsync(CommentPagingRequestModel request);
        Task<ServiceResponse<bool>> DeleteReplyAsync(int id, string identityUserId);
    }
}
