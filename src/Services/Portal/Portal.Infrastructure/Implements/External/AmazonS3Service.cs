using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Portal.Domain.Interfaces.External;
using Portal.Domain.Models.ImageUploadModels;

namespace Portal.Infrastructure.Implements.External
{
    public class AmazonS3Service : IAmazonS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName = "s3.codegota.me";

        public AmazonS3Service(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        [Obsolete("Replaced by UploadImageAsync in the future")]
        public async Task<ImageUploadResultModel> UploadImageAsync(ImageUploadRequestModel requestModel, string? prefix = null)
        {
            var bucketExists = await _s3Client.DoesS3BucketExistAsync(_bucketName);
            if (!bucketExists)
            {
                return new ImageUploadResultModel();
            }

            var putRequest = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = string.IsNullOrEmpty(prefix) ? requestModel.FileName : $"{prefix.TrimEnd('/')}/{requestModel.FileName}",
                InputStream = new MemoryStream(requestModel.ImageData),
                CannedACL = S3CannedACL.PublicRead
            };

            var uploadResult = await _s3Client.PutObjectAsync(putRequest);
            var result = new ImageUploadResultModel
            {
                FileName = requestModel.FileName,
                AbsoluteUrl = uploadResult.HttpStatusCode == HttpStatusCode.OK ? $"https://{_bucketName}.s3.amazonaws.com/{putRequest.Key}" : null,
                RelativeUrl = uploadResult.HttpStatusCode == HttpStatusCode.OK ? putRequest.Key : null,
                OrderBy = requestModel.OrderBy,
                IsPublic = requestModel.IsPublic
            };

            return result;
        }

        [Obsolete("Replaced by DoesS3BucketExistV2Async in the future")]
        public async Task<List<ImageUploadResultModel>> BulkUploadImagesAsync(List<ImageUploadRequestModel> requestModels, string? prefix = null)
        {
            var bucketExists = await _s3Client.DoesS3BucketExistAsync(_bucketName);
            if (!bucketExists)
            {
                return new List<ImageUploadResultModel>();
            }

            var result = new List<ImageUploadResultModel>();
            foreach (var requestModel in requestModels)
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = string.IsNullOrEmpty(prefix) ? requestModel.FileName : $"{prefix.TrimEnd('/')}/{requestModel.FileName}",
                    InputStream = new MemoryStream(requestModel.ImageData),
                    CannedACL = S3CannedACL.PublicRead
                };

                var uploadResult = await _s3Client.PutObjectAsync(putRequest);
                result.Add(new ImageUploadResultModel
                {
                    FileName = requestModel.FileName,
                    AbsoluteUrl = uploadResult.HttpStatusCode == HttpStatusCode.OK ? $"https://{_bucketName}.s3.amazonaws.com/{putRequest.Key}" : null,
                    RelativeUrl = uploadResult.HttpStatusCode == HttpStatusCode.OK ? putRequest.Key : null,
                    OrderBy = requestModel.OrderBy,
                    IsPublic = requestModel.IsPublic
                });
            }

            return result;
        }
    }
}
