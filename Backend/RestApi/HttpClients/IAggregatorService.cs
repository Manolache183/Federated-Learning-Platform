namespace RestApi.HttpClients // don t use multiple namespaces
{
    public interface IAggregatorService
    {
        public abstract Task<HttpResponseMessage> PullLearningModelAsync();
        public abstract Task<HttpResponseMessage> SendWeightsAsync(MemoryStream memoryStream, string filename);
    }
}
