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
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IAmazonS3 _s3Client;

        public FilesController(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBucketAsync(string bucketName)
        {
            var awsCredentials = new BasicAWSCredentials("AKIASTK663DBYPX4K26W", "zQEM9Db8KDxd7Lef35A/y7MMkULrK6A9PP8DJ8S1");
            var s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.USWest2); // Replace with your desired region
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            if (bucketExists) return BadRequest($"Bucket {bucketName} already exists.");
            await _s3Client.PutBucketAsync(bucketName);
            return Created("buckets", $"Bucket {bucketName} created.");
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

        [HttpGet("preview")]
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
    }
}
