using Common.Models;
using Portal.Domain.AggregatesModel.AlbumAggregate;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.AggregatesModel.UserAggregate;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Models.CommentModels;
using Portal.Domain.SeedWork;
using Portal.Infrastructure.Helpers;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class CommentService : ICommentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Comment> _commentRepository;
        private readonly IGenericRepository<Album> _albumRepository;
        private readonly IGenericRepository<User> _userRepository;
        private readonly IGenericRepository<Collection> _collectionRepository;

        public CommentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _commentRepository = _unitOfWork.Repository<Comment>();
            _albumRepository = _unitOfWork.Repository<Album>();
            _userRepository = _unitOfWork.Repository<User>();
            _collectionRepository = _unitOfWork.Repository<Collection>();
        }

        public async Task<ServiceResponse<CommentModel>> CreateAsync(CommentRequestModel request, string identityUserId)
        {
            var user = await _userRepository.GetByIdentityUserIdAsync(identityUserId);
            if (user == null)
            {
                return new ServiceResponse<CommentModel>("error_user_not_found");
            }

            var album = await _albumRepository.GetByIdAsync(request.AlbumId);
            if (album == null)
            {
                return new ServiceResponse<CommentModel>("error_album_not_found");
            }

            Collection? collection;
            if (request.CollectionId.HasValue)
            {
                collection = await _collectionRepository.GetByIdAsync(request.CollectionId.Value);
                if (collection == null)
                {
                    return new ServiceResponse<CommentModel>("error_collection_not_found");
                }
            }

            var comment = new Comment
            {
                Text = request.Text,
                CollectionId = request.CollectionId,
                UserId = user.Id,
                AlbumId = request.AlbumId
            };

            _commentRepository.Add(comment);
            await _unitOfWork.SaveChangesAsync();

            // Response
            var response = new CommentModel
            {
                Id = comment.Id,
                Text = comment.Text,
                UserId = comment.UserId,
                AlbumId = comment.AlbumId,
                CollectionId = comment.CollectionId,
                CreatedOnUtc = comment.CreatedOnUtc,
                UpdatedOnUtc = comment.UpdatedOnUtc,
                FullName = user.FullName,
                UserName = user.UserName
            };

            return new ServiceResponse<CommentModel>(response);
        }

        public async Task<ServiceResponse<CommentModel>> UpdateAsync(int id, CommentRequestModel request, string identityUserId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null || comment.IsDeleted)
            {
                return new ServiceResponse<CommentModel>("error_comment_not_found");
            }

            var user = await _userRepository.GetByIdentityUserIdAsync(identityUserId);
            if (user == null)
            {
                return new ServiceResponse<CommentModel>("error_user_not_found");
            }

            if (comment.UserId != user.Id)
            {
                return new ServiceResponse<CommentModel>("error_comment_not_belog_current_user");
            }

            var album = await _albumRepository.GetByIdAsync(request.AlbumId);
            if (album == null)
            {
                return new ServiceResponse<CommentModel>("error_album_not_found");
            }

            if (comment.AlbumId != request.AlbumId)
            {
                return new ServiceResponse<CommentModel>("error_comment_not_belong_current_album");
            }

            Collection? collection;
            if (request.CollectionId.HasValue)
            {
                collection = await _collectionRepository.GetByIdAsync(request.CollectionId.Value);
                if (collection == null)
                {
                    return new ServiceResponse<CommentModel>("error_collection_not_found");
                }

                if (comment.CollectionId != request.CollectionId)
                {
                    return new ServiceResponse<CommentModel>("error_comment_not_belong_current_collection");
                }
            }

            comment.Text = request.Text;
            _commentRepository.Update(comment);
            await _unitOfWork.SaveChangesAsync();

            // Response
            var response = new CommentModel
            {
                Id = comment.Id,
                Text = comment.Text,
                UserId = comment.UserId,
                AlbumId = comment.AlbumId,
                CollectionId = comment.CollectionId,
                CreatedOnUtc = comment.CreatedOnUtc,
                UpdatedOnUtc = comment.UpdatedOnUtc,
                FullName = user.FullName,
                UserName = user.UserName
            };

            return new ServiceResponse<CommentModel>(response);
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id, string identityUserId)
        {
            var comment = await _commentRepository.GetByIdAsync(id);
            if (comment == null || comment.IsDeleted)
            {
                return new ServiceResponse<bool>("error_comment_not_found");
            }

            var user = await _userRepository.GetByIdentityUserIdAsync(identityUserId);
            if (user == null)
            {
                return new ServiceResponse<bool>("error_user_not_found");
            }

            if (comment.UserId != user.Id)
            {
                return new ServiceResponse<bool>("error_comment_not_belog_current_user");
            }

            comment.IsDeleted = true;
            _commentRepository.Update(comment);
            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponse<bool>(true);
        }

        public async Task<ServiceResponse<PagingCommonResponse<CommentPagingResposneModel>>> GetPagingAsync(CommentPagingRequestModel request)
        {
            var parameters = new Dictionary<string, object?>
            {
                { "pageNumber", request.PageNumber },
                { "pageSize", request.PageSize },
                { "searchTerm", request.SearchTerm },
                { "sortColumn", request.SortColumn },
                { "sortDirection", request.SortDirection },
                { "albumId", request.AlbumId },
                { "collectionId", request.CollectionId },
                { "userId", request.UserId },
                { "isReply", request.IsReply },
                { "parentCommentId", request.ParentCommentId }
            };
            var result = await _unitOfWork.QueryAsync<CommentPagingResposneModel>("Comment_All_Paging", parameters);

            var record = result.Find(o => o.IsTotalRecord);
            if (record == null)
            {
                return new ServiceResponse<PagingCommonResponse<CommentPagingResposneModel>>(new PagingCommonResponse<CommentPagingResposneModel>
                {
                    RowNum = 0,
                    Data = new List<CommentPagingResposneModel>()
                });
            }

            result.Remove(record);
            return new ServiceResponse<PagingCommonResponse<CommentPagingResposneModel>>(new PagingCommonResponse<CommentPagingResposneModel>
            {
                RowNum = record.RowNum,
                Data = result
            });
        }
    }
}
