using Common.Interfaces;
using Common.Models;
using Common.ValueObjects;
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
        private readonly IGenericRepository<ReplyComment> _replyCommentRepository;
        private readonly IRedisService _redisService;

        public CommentService(IUnitOfWork unitOfWork, IRedisService redisService)
        {
            _unitOfWork = unitOfWork;
            _commentRepository = _unitOfWork.Repository<Comment>();
            _albumRepository = _unitOfWork.Repository<Album>();
            _userRepository = _unitOfWork.Repository<User>();
            _collectionRepository = _unitOfWork.Repository<Collection>();
            _replyCommentRepository = _unitOfWork.Repository<ReplyComment>();
            _redisService = redisService;
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

            Comment? parentComment = null;
            if (request.ParentCommentId.HasValue)
            {
                parentComment = await _commentRepository.GetByIdAsync(request.ParentCommentId.Value);
                if (parentComment == null)
                {
                    return new ServiceResponse<CommentModel>("error_parent_comment_not_found");
                }
            }

            CommentModel? response;
            // Comment will not have parent
            if (!request.ParentCommentId.HasValue)
            {
                var comment = new Comment
                {
                    Text = request.Text,
                    CollectionId = request.CollectionId,
                    UserId = user.Id,
                    AlbumId = request.AlbumId
                };

                _commentRepository.Add(comment);
                await _unitOfWork.SaveChangesAsync();

                await BuildPagingOneCacheAsync(album.Id);

                // Response
                response = new CommentModel
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
            }
            else
            {
                var replyComment = new ReplyComment
                {
                    Text = request.Text,
                    UserId = user.Id,
                    CommentId = request.ParentCommentId.Value
                };

                _replyCommentRepository.Add(replyComment);
                await _unitOfWork.SaveChangesAsync();

                if (parentComment != null)
                {
                    await BuildPagingOneCacheAsync(parentComment.AlbumId);
                }

                // Response
                response = new CommentModel
                {
                    Id = replyComment.Id,
                    Text = replyComment.Text,
                    UserId = replyComment.UserId,
                    CreatedOnUtc = replyComment.CreatedOnUtc,
                    UpdatedOnUtc = replyComment.UpdatedOnUtc,
                    FullName = user.FullName,
                    UserName = user.UserName
                };
            }

            return new ServiceResponse<CommentModel>(response);
        }

        public async Task<ServiceResponse<CommentModel>> UpdateAsync(int id, CommentRequestModel request, string identityUserId)
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


            CommentModel? response = new CommentModel();
            if (!request.ParentCommentId.HasValue)
            {
                var comment = await _commentRepository.GetByIdAsync(id);
                if (comment == null || comment.IsDeleted)
                {
                    return new ServiceResponse<CommentModel>("error_comment_not_found");
                }

                if (comment.AlbumId != request.AlbumId)
                {
                    return new ServiceResponse<CommentModel>("error_comment_not_belong_current_album");
                }

                if (comment.UserId != user.Id)
                {
                    return new ServiceResponse<CommentModel>("error_comment_not_belog_current_user");
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

                if (request.CanUpdate)
                {
                    comment.Text = request.Text;
                    _commentRepository.Update(comment);
                    await _unitOfWork.SaveChangesAsync();

                    await BuildPagingOneCacheAsync(album.Id);

                    // Response
                    response = new CommentModel
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
                }
            }
            else
            {
                var comment = await _replyCommentRepository.GetByIdAsync(id);
                if (comment == null || comment.IsDeleted)
                {
                    return new ServiceResponse<CommentModel>("error_comment_not_found");
                }

                if (comment.UserId != user.Id)
                {
                    return new ServiceResponse<CommentModel>("error_comment_not_belog_current_user");
                }

                if (comment.CommentId != request.ParentCommentId)
                {
                    return new ServiceResponse<CommentModel>("error_comment_not_belong_current_comment");
                }

                var parentComment = await _commentRepository.GetByIdAsync(comment.CommentId);
                if (parentComment == null || parentComment.IsDeleted)
                {
                    return new ServiceResponse<CommentModel>("error_parent_comment_not_found");
                }

                if (request.CanUpdate)
                {
                    comment.Text = request.Text;
                    _replyCommentRepository.Update(comment);
                    await _unitOfWork.SaveChangesAsync();

                    // Response
                    response = new CommentModel
                    {
                        Id = comment.Id,
                        Text = comment.Text,
                        UserId = comment.UserId,
                        CreatedOnUtc = comment.CreatedOnUtc,
                        UpdatedOnUtc = comment.UpdatedOnUtc,
                        FullName = user.FullName,
                        UserName = user.UserName
                    };
                }
            }

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

            comment.IsDeleted = true;
            _commentRepository.Update(comment);
            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponse<bool>(true);
        }

        public async Task<ServiceResponse<bool>> DeleteReplyAsync(int id, string identityUserId)
        {
            var replyComment = await _replyCommentRepository.GetByIdAsync(id);
            if (replyComment == null || replyComment.IsDeleted)
            {
                return new ServiceResponse<bool>("error_comment_not_found");
            }

            var user = await _userRepository.GetByIdentityUserIdAsync(identityUserId);
            if (user == null)
            {
                return new ServiceResponse<bool>("error_user_not_found");
            }

            if (replyComment.UserId != user.Id)
            {
                return new ServiceResponse<bool>("error_comment_not_belog_current_user");
            }

            replyComment.IsDeleted = true;
            _replyCommentRepository.Update(replyComment);
            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponse<bool>(true);
        }

        public async Task<ServiceResponse<PagingCommonResponse<CommentPagingResposneModel>>> BuildPagingOneCacheAsync(int albumId)
        {
            var parameters = new Dictionary<string, object?>
                {
                    { "pageNumber", 1 },
                    { "pageSize", 5 },
                    { "searchTerm", string.Empty },
                    { "sortColumn", "createdOnUtc" },
                    { "sortDirection", "desc" },
                    { "albumId", albumId },
                    { "collectionId", null },
                    { "userId", null },
                };

            var result = await _unitOfWork.QueryAsync<CommentPagingResposneModel>("Comment_All_Paging", parameters);
            await _redisService.SetAsync(string.Format(Const.RedisCacheKey.ComicCommentPageOneCache, albumId), result, 60);

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

        public async Task<ServiceResponse<PagingCommonResponse<CommentPagingResposneModel>>> GetPagingAsync(CommentPagingRequestModel request)
        {
            List<CommentPagingResposneModel>? result;

            if (request.IsReply.HasValue && request.IsReply.Value)
            {
                var parameters = new Dictionary<string, object?>
                {
                    { "pageNumber", request.PageNumber },
                    { "pageSize", request.PageSize },
                    { "searchTerm", request.SearchTerm },
                    { "sortColumn", request.SortColumn },
                    { "sortDirection", request.SortDirection },
                    { "userId", request.UserId },
                    { "commentId", request.ParentCommentId }
                };
                result = await _unitOfWork.QueryAsync<CommentPagingResposneModel>("ReplyComment_All_Paging", parameters);
            }
            else
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
                };

                // Cache comment first page
                if (request.PageNumber == 1 &&
                    request.PageSize == 5 &&
                    string.IsNullOrEmpty(request.SearchTerm) &&
                    !request.CollectionId.HasValue &&
                    !request.UserId.HasValue)
                {
                    result = await _redisService.GetByFunctionAsync(string.Format(Const.RedisCacheKey.ComicCommentPageOneCache, request.AlbumId), 60, () =>
                    {
                        return _unitOfWork.QueryAsync<CommentPagingResposneModel>("Comment_All_Paging", parameters);
                    });
                }
                else
                {
                    result = await _unitOfWork.QueryAsync<CommentPagingResposneModel>("Comment_All_Paging", parameters);
                }
            }

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
