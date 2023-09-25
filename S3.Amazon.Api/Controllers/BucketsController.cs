using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Mvc;

namespace S3.Amazon.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BucketsController : ControllerBase
{
    private readonly IAmazonS3 _s3Client;

    public BucketsController(IAmazonS3 s3Client)
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
    public async Task<IActionResult> GetAllBucketAsync()
    {
        var awsCredentials = new BasicAWSCredentials("AKIASTK663DBYPX4K26W", "zQEM9Db8KDxd7Lef35A/y7MMkULrK6A9PP8DJ8S1");
        var s3Client = new AmazonS3Client(awsCredentials, RegionEndpoint.USWest2); // Replace with your desired region

        var data = await s3Client.ListBucketsAsync();
        var buckets = data.Buckets.Select(b => { return b.BucketName; });
        return Ok(buckets);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteBucketAsync(string bucketName)
    {
        await _s3Client.DeleteBucketAsync(bucketName);
        return NoContent();
    }
}
