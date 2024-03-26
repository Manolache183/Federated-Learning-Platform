namespace Logger.DataModels
{
    public record LogItem(Guid id, string microserviceName, DateTime timestamp);
}
