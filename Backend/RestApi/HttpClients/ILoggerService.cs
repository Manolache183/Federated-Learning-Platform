namespace RestApi.HttpClients
{
    public interface ILoggerService
    {
        public abstract Task<HttpResponseMessage> LogAsync();
        public abstract Task<HttpResponseMessage> PingAsync();
    }
}
