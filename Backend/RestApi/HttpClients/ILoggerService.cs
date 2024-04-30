using RestApi.DTOS;

namespace RestApi.HttpClients
{
    public interface ILoggerService
    {
        public abstract Task<HttpResponseMessage> LogAsync();
        public abstract Task<HttpResponseMessage> PingAsync();
        public abstract Task<bool> AddFileMetadata(string fileName, Guid FirebaseStorageID);
        public abstract Task<FileMetadata?> GetFileMetadata(string fileName);
        public abstract Task<bool> UpdateFileMetadata(string fileName, string filePath, Guid FirebaseStorageID);
        public abstract Task<bool> DeleteFileMetadata(string fileName);
        public abstract Task<bool> SwapModelFiles(string latestModelFirebaseStorageID);
    }
}
