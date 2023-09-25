using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using S3.Amazon.Api.Models;

namespace S3.Amazon.Api.Controllers
{
    [Route("files")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;

        public FilesController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBucketAsync(IFormFile file, string bucketName, string? prefix)
        {
            var awsCredentials = new BasicAWSCredentials("AKIASTK663DBYPX4K26W", "zQEM9Db8KDxd7Lef35A/y7MMkULrK6A9PP8DJ8S1");
            var _s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.APSoutheast2);

            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists) return NotFound($"Bucket {bucketName} does not exist.");
            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = string.IsNullOrEmpty(prefix) ? file.FileName : $"{prefix?.TrimEnd('/')}/{file.FileName}",
                InputStream = file.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", file.ContentType);
            await _s3Client.PutObjectAsync(request);
            return Ok($"File {prefix}/{file.FileName} uploaded to S3 successfully!");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFilesAsync()
        {
            var awsCredentials = new BasicAWSCredentials("AKIASTK663DBYPX4K26W", "zQEM9Db8KDxd7Lef35A/y7MMkULrK6A9PP8DJ8S1");
            var _s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.APSoutheast2);
            string bucketName = "oxirgisi";
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists) return NotFound($"Bucket {bucketName} does not exist.");
            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,

            };
            var result = await _s3Client.ListObjectsV2Async(request);
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddMinutes(1)
                };
                return new S3ObjectDto()
                {
                    Name = s.Key.ToString(),
                    PresignedUrl = _s3Client.GetPreSignedURL(urlRequest),
                };
            });
            return Ok(s3Objects);
        }

        [HttpGet("getByPath")]
        public async Task<IActionResult> GetFileByKeyAsync(string bucketName, string key)
        {
            var awsCredentials = new BasicAWSCredentials("AKIASTK663DBYPX4K26W", "zQEM9Db8KDxd7Lef35A/y7MMkULrK6A9PP8DJ8S1");
            var _s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.APSoutheast2);
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists) return NotFound($"Bucket {bucketName} does not exist.");
            var s3Object = await _s3Client.GetObjectAsync(bucketName, key);
            return File(s3Object.ResponseStream, s3Object.Headers.ContentType);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFileAsync(string bucketName, string key)
        {
            var awsCredentials = new BasicAWSCredentials("AKIASTK663DBYPX4K26W", "zQEM9Db8KDxd7Lef35A/y7MMkULrK6A9PP8DJ8S1");
            var _s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.APSoutheast2);
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists) return NotFound($"Bucket {bucketName} does not exist");
            await _s3Client.DeleteObjectAsync(bucketName, key);
            return NoContent();
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadFileByKeyAsync(string bucketName, string key)
        {
            var awsCredentials = new BasicAWSCredentials("AKIASTK663DBYPX4K26W", "zQEM9Db8KDxd7Lef35A/y7MMkULrK6A9PP8DJ8S1");
            var _s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.APSoutheast2);
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (!bucketExists) return NotFound($"Bucket {bucketName} does not exist");

            try
            {
                var s3Object = await _s3Client.GetObjectAsync(bucketName, key);
                var fileStream = s3Object.ResponseStream;
                var contentType = s3Object.Headers.ContentType;
                var fileName = key; // You can set a different file name here if needed

                return File(fileStream, contentType, fileName);
            }
            catch (AmazonS3Exception ex)
            {
                // Handle S3-specific exceptions here if needed
                return BadRequest($"Error downloading file: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions here if needed
                return BadRequest($"Error downloading file: {ex.Message}");
            }
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateFileByKeyAsync(IFormFile file, string bucketName, string key)
        {
            var awsCredentials = new BasicAWSCredentials("AKIASTK663DBYPX4K26W", "zQEM9Db8KDxd7Lef35A/y7MMkULrK6A9PP8DJ8S1");
            var _s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.APSoutheast2);
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            if (!bucketExists) return NotFound($"Bucket {bucketName} does not exist");

            try
            {
                var request = new PutObjectRequest()
                {
                    BucketName = bucketName,
                    Key = key,
                    InputStream = file.OpenReadStream(),
                    ContentType = file.ContentType
                };

                await _s3Client.PutObjectAsync(request);

                return Ok($"File {key} in bucket {bucketName} has been updated successfully!");
            }
            catch (AmazonS3Exception ex)
            {
                // Handle S3-specific exceptions here if needed
                return BadRequest($"Error updating file: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other exceptions here if needed
                return BadRequest($"Error updating file: {ex.Message}");
            }
        }

    }
}
