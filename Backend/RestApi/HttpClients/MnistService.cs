namespace RestApi.HttpClients
{
    public class MnistService : IAggregatorService
    {
        private readonly HttpClient _httpClient;
        
        public MnistService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<HttpResponseMessage> PullLearningModelAsync() // parse the results here?
        {
            const string url = "/learning/request_model";
            return await _httpClient.GetAsync(url); 
        }

        public async Task<HttpResponseMessage> SendWeightsAsync(MemoryStream memoryStream, string filename) // parse the content here?
        {
            const string url = "/learning/send_weights";
            using (var content = new MultipartFormDataContent()) // is it possible to send the file without reading it into memory?
            {
                content.Add(new StreamContent(memoryStream), "file", filename);
                return await _httpClient.PostAsync(url, content);
            }
        }

    }
}
