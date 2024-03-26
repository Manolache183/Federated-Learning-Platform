namespace RestApi.HttpClients
{
    public interface ILoggerService
    {
        public abstract Task<HttpResponseMessage> LogAsync();
    }
}
