namespace Logger.Firebase
{
    public record LogItem (Guid id, string microserviceName, string message, DateTime timestamp);
    public record LogItemReceive (string microserviceName, string message);

    public record FileMetadata(string fileName, String firebaseStorageID, DateTime leastAccessed);
    public record FileMetadataReceive(string fileName, String firebaseStorageID);
    public record FileMetadataSend(string firebaseStorageID, string leastAccessed);
}
