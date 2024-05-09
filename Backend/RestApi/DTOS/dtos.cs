namespace RestApi.DTOS
{
    public record LogItem(string microserviceName, string message);

    public record FileMetadata(string firebaseStorageID, string leastAccessed);
    public record FileMetadataSend(string fileName, Guid firebaseStorageID);
    public record FileContent(string content);

    public record UserData(string email, string password, string role);
    public record UserToken(string token);
}
