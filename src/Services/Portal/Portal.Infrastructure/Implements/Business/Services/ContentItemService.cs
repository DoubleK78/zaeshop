using Common;
using Common.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using Portal.Domain.AggregatesModel.CollectionAggregate;
using Portal.Domain.Interfaces.Business.Services;
using Portal.Domain.Interfaces.External;
using Portal.Domain.Models.ContentItemModels;
using Portal.Domain.Models.ImageUploadModels;
using Portal.Domain.SeedWork;

namespace Portal.Infrastructure.Implements.Business.Services
{
    public class ContentItemService : IContentItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGenericRepository<Collection> _collectionRepository;
        private readonly IGenericRepository<ContentItem> _contentItemRepository;
        private readonly IAmazonS3Service _amazonS3Service;

        public ContentItemService(
            IUnitOfWork unitOfWork,
            IAmazonS3Service amazonS3Service)
        {
            _unitOfWork = unitOfWork;
            _contentItemRepository = unitOfWork.Repository<ContentItem>();
            _collectionRepository = unitOfWork.Repository<Collection>();
            _amazonS3Service = amazonS3Service;
        }

        private static string GetBoundary(MediaTypeHeaderValue contentType)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

            if (string.IsNullOrWhiteSpace(boundary))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            return boundary;
        }

        public async Task<ServiceResponse<bool>> CreateContentItemsAsync(int collectionId, Stream stream, string contentType)
        {
            // Retrieve the existing collection entity by ID
            var existingCollection = await _collectionRepository.GetByIdAsync(collectionId);
            if (existingCollection == null)
            {
                return new ServiceResponse<bool>("error_collection_not_found");
            }

            // Check if Collection has any content item, that should be update instead create
            var isExistingContentItems = existingCollection.ContentItems.Count > 0;
            if (isExistingContentItems)
            {
                return new ServiceResponse<bool>("error_content_type_not_empty");
            }

            var results = new List<ImageUploadResultModel>();

            #region Handle large files from Streams
            var boundary = GetBoundary(MediaTypeHeaderValue.Parse(contentType));
            var multipartReader = new MultipartReader(boundary, stream);
            var section = await multipartReader.ReadNextSectionAsync();

            while (section != null)
            {
                var fileSection = section.AsFileSection();
                if (fileSection != null)
                {
                    var result = await _amazonS3Service.UploadImageAsync(new ImageUploadRequestModel
                    {
                        FileName = fileSection.FileName,
                        ImageData = await CommonHelper.GetFileBytesByStreamAsync(fileSection.FileStream)
                    }, $"{existingCollection.Album.FriendlyName}/{existingCollection.FriendlyName}");

                    results.Add(result);
                }

                section = await multipartReader.ReadNextSectionAsync();
            }
            #endregion

            // Store database
            var addItems = results.OrderBy(r => r.FileName).Select((x, index) => new ContentItem
            {
                CollectionId = collectionId,
                Name = x.FileName,
                OriginalUrl = x.AbsoluteUrl,
                DisplayUrl = $"https://s3.codegota.me/{x.RelativeUrl}",
                RelativeUrl = x.RelativeUrl,
                OrderBy = index
            }).OrderBy(x => x.OrderBy).ToList();

            _contentItemRepository.AddRange(addItems);
            await _unitOfWork.SaveChangesAsync();

            return new ServiceResponse<bool>(true);
        }

        public async Task<ServiceResponse<bool>> UpdateContentItemsAsync(int collectionId, ContentItemUpdateRequestModel model)
        {
            var createContentItems = new List<ContentItem>();
            var updateContentItems = new List<ContentItem>();
            var deleteContentItems = new List<ContentItem>();

            // Retrieve the existing collection entity by ID
            var existingCollection = await _collectionRepository.GetByIdAsync(collectionId);
            if (existingCollection == null)
            {
                return new ServiceResponse<bool>("error_collection_not_found");
            }

            // Check when new collection - before create then update
            var contentItems = await _contentItemRepository.GetQueryable().Where(x => x.CollectionId == collectionId).ToListAsync();
            if (contentItems == null || contentItems.Count == 0)
            {
                return new ServiceResponse<bool>("error_content_type_empty");
            }

            // Update exists content item by id
            if (model.ExistsItems?.Count > 0)
            {
                // Items have changed image, re-upload and replace name and url
                var amazonBulkUploadModels = model.ExistsItems.Where(o => !string.IsNullOrEmpty(o.FileName) && o.FileData != null).Select(x => new ImageUploadRequestModel
                {
                    FileName = x.FileName!,
                    ImageData = x.FileData!,
                    OrderBy = x.OrderBy,
                    IsPublic = x.IsPublic
                }).ToList();

                var result = await _amazonS3Service.BulkUploadImagesAsync(amazonBulkUploadModels, $"{existingCollection.Album.FriendlyName}/{existingCollection.FriendlyName}");
                foreach (var item in result)
                {
                    var reUploadItem = model.ExistsItems.Find(x => x.FileName == item.FileName);
                    var contentItem = contentItems.Find(x => x.Id == reUploadItem?.Id);
                    if (contentItem != null)
                    {
                        contentItem.Name = item.FileName;
                        contentItem.OriginalUrl = item.AbsoluteUrl;
                        contentItem.DisplayUrl = item.AbsoluteUrl;
                        contentItem.RelativeUrl = item.RelativeUrl;
                    }
                }

                foreach (var item in model.ExistsItems)
                {
                    var contentItem = contentItems.Find(x => x.Id == item.Id);
                    if (contentItem != null)
                    {
                        contentItem.OrderBy = item.OrderBy;
                        updateContentItems.Add(contentItem);
                    }
                }

                // Content are not longer use, we will remove them
                var existingContentItemIds = updateContentItems.ConvertAll(x => x.Id);
                deleteContentItems.AddRange(contentItems.Where(x => !existingContentItemIds.Contains(x.Id)));
            }
            else
            {
                deleteContentItems.AddRange(contentItems);
            }

            // Build and Upload to image services
            if (model.Items?.Count > 0)
            {
                var amazonBulkUploadModels = model.Items.Where(o => !string.IsNullOrEmpty(o.FileName) && o.FileData != null).Select(x => new ImageUploadRequestModel
                {
                    FileName = x.FileName!,
                    ImageData = x.FileData!,
                    OrderBy = x.OrderBy,
                    IsPublic = x.IsPublic
                }).ToList();

                var result = await _amazonS3Service.BulkUploadImagesAsync(amazonBulkUploadModels, $"{existingCollection.Album.Title}/{existingCollection.Title}");
                foreach (var newItem in result)
                {
                    createContentItems.Add(new ContentItem
                    {
                        CollectionId = collectionId,
                        Name = newItem.FileName,
                        OriginalUrl = newItem.AbsoluteUrl,
                        DisplayUrl = newItem.AbsoluteUrl,
                        RelativeUrl = newItem.RelativeUrl,
                        OrderBy = newItem.OrderBy
                    });
                }
            }

            _contentItemRepository.DeleteRange(deleteContentItems);

            if (updateContentItems.Count > 0)
            {
                _contentItemRepository.UpdateRange(updateContentItems);
            }

            if (createContentItems.Count > 0)
            {
                _contentItemRepository.AddRange(createContentItems);
            }

            await _unitOfWork.SaveChangesAsync();
            return new ServiceResponse<bool>(true);
        }
    }
}
