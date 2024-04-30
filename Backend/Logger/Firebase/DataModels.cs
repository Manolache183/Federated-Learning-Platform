namespace Logger.Firebase
{
    public record LogItem (Guid Id, string MicroserviceName, DateTime Timestamp);
    public record LogItemDto (string MicroserviceName);

    public record FileMetadata(string FileName, Guid FirebaseStorageID, DateTime LeastAccesed);
    public record FileMetadataReceive(string FileName, Guid FirebaseStorageID);
    public record FileMetadataSend(string FirebaseStorageID, string LeastAccesed);
}
