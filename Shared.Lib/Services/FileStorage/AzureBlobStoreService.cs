using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

namespace Shared.Lib.Services.FileStorage
{
    public abstract class AzureBlobStoreService : FileStoreServiceBase
    {
        protected readonly BlobContainerClient _blobContainerClient;
        private readonly BlobServiceClient _defaultClient;

        public AzureBlobStoreService(BlobServiceClient defaultClient, string containerName)
        {
            _blobContainerClient = defaultClient.GetBlobContainerClient(containerName);
            _defaultClient = defaultClient;
        }

        public override async Task<bool> Exists(string fileName, string? versionId)
        {
            var query = _blobContainerClient
                .GetBlobClient(fileName);

            if (versionId is not null)
            {
                query.WithVersion(versionId);
            }

            return await query.ExistsAsync();
        }

        public override async Task<string?> GetVersionId(string fileName)
        {
            var query = _blobContainerClient
                .GetBlobClient(fileName);

            var result = await query.GetPropertiesAsync();

            if (result == null)
            {
                return null;
            }

            return result.Value.VersionId;
        }
        public override async Task<IDictionary<string, string>?> GetTags(string fileName)
        {
            var query = _blobContainerClient
                .GetBlobClient(fileName);

            var result = await query.GetTagsAsync();

            if (result == null)
            {
                return null;
            }

            return result.Value.Tags;
        }
        public override async Task<Stream?> GetFileAsync(string fileName, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            var createResponse = await _blobContainerClient.CreateIfNotExistsAsync();
            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                await _blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var blob = _blobContainerClient.GetBlobClient(fileName);

            if (blob == null)
            {
                return null;
            }

            //TODO: Add lease/legalHold before start downloading
            var blobOptions = new BlobDownloadOptions
            { };

            if (progress != null)
            {
                blobOptions.ProgressHandler = progress;
            }

            var result = await blob.DownloadStreamingAsync(blobOptions, cancellationToken);

            if (result == null)
            {
                return null;
            }

            //TODO: Remove lease/legalHold after finish downloading
            return result.Value.Content;
        }
        public override async Task<bool> PutFileAsync(
            Stream file, string fileName, Dictionary<string, string> tags, IProgress<long> progress = null, CancellationToken cancellationToken = default)
        {
            var createResponse = await _blobContainerClient.CreateIfNotExistsAsync();
            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                await _blobContainerClient.SetAccessPolicyAsync(PublicAccessType.Blob);

            var blob = _blobContainerClient.GetBlobClient(fileName);
            var blobOptions = new BlobUploadOptions
            {
                Tags = tags
            };

            if (progress != null)
            {
                blobOptions.ProgressHandler = progress;
            }

            var uploadResponse = await blob.UploadAsync
                (
                    file, blobOptions, cancellationToken
                );

            if (uploadResponse != null && uploadResponse.GetRawResponse().Status == 201)
                return true;
            return false;
        }
    }

    public class InputStorageService : AzureBlobStoreService
    {
        private const string CONTAINER_NAME = "mdm-b-input";

        public InputStorageService(BlobServiceClient defaultClient)
            : base(defaultClient, CONTAINER_NAME)
        { }
    }

    public class OutputStorageService : AzureBlobStoreService
    {
        private const string CONTAINER_NAME = "mdm-b-output";

        public OutputStorageService(BlobServiceClient defaultClient)
            : base(defaultClient, CONTAINER_NAME)
        { }
    }
}
