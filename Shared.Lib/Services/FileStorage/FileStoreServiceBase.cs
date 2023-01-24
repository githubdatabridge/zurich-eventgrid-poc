namespace Shared.Lib.Services.FileStorage
{
    public abstract class FileStoreServiceBase : IFileStoreService
    {
        //public abstract Task<bool> DeleteFileAsync(string fileName, string versionId = "");

        public abstract Task<Stream?> GetFileAsync(string fileName, IProgress<long> progress = null, CancellationToken cancellationToken = default);

        //public abstract string GetFilePath(string fileName);
        //public abstract string GetFileUrl(string fileName, int days = 356);
        public abstract Task<bool> Exists(string fileName, string? versionId);

        public abstract Task<string?> GetVersionId(string fileName);

        public abstract Task<bool> PutFileAsync(Stream file, string fileName, Dictionary<string, string> tags, IProgress<long> progress = null, CancellationToken cancellationToken = default);

        public abstract Task<IDictionary<string, string>?> GetTags(string fileName);
        //public abstract Task<bool> UploadFileAsync(byte[] file, string fileName, string fileContentType, IProgress<long> progress);
    }
}
