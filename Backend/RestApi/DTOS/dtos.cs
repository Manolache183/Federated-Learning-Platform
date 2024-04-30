namespace RestApi.DTOS
{
    public record LogItemDto(string microserviceName);

    public record FileMetadata(string firebaseStorageID, string leastAccesed);
    public record FileMetadataSend(string fileName, Guid firebaseStorageID);
    public record FileContent(string content);
}
