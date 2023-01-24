namespace Shared.Lib.Services.FileStorage
{
    public interface IFileStoreService
    {
        Task<bool> PutFileAsync(Stream file, string fileName, Dictionary<string, string> tags, IProgress<long> progress = null, CancellationToken cancellationToken = default);
        //Task<bool> UploadFileAsync(byte[] file, string fileName, string fileContentType, IProgress<long> progress);
        //Task<bool> DeleteFileAsync(string fileName, string versionId = "");
        Task<bool> Exists(string fileName, string? versionId);
        Task<string?> GetVersionId(string fileName);
        Task<IDictionary<string,string>?> GetTags(string fileName); 
        Task<Stream?> GetFileAsync(string fileName, IProgress<long> progress = null, CancellationToken cancellationToken = default);
        //string GetFileUrl(string fileName, int days = 356);
        //string GetFilePath(string fileName);
    }
}


