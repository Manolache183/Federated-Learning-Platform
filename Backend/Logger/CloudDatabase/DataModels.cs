namespace Logger.CloudDatabase
{
    public record LogItem (Guid Id, string MicroserviceName, DateTime Timestamp);
    public record LogItemDto (string MicroserviceName);
}
