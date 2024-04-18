using Firebase.Storage;

namespace RestApi.CloudDatabase
{
    public class DatabaseService
    {
        private readonly string _path = AppDomain.CurrentDomain.BaseDirectory + @"federated-learning-platf-c15c0-firebase-adminsdk-slcw0-90585331c4.json";
        private const string _bucket = "federated-learning-platf-c15c0.appspot.com";
        private const string _folder = "models/";
        private readonly FirebaseStorage _db;
        private readonly HttpClient _httpClient;
        public DatabaseService()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _path);
            _db = new FirebaseStorage(_bucket);
            _httpClient = new HttpClient();
        }

        public async Task<string> UploadModel(string modelPath)
        {
            var model = await _db.Child(_folder).Child("test_upload_file.txt").PutAsync(File.OpenRead(modelPath));
            return model;
        }

        public async Task PrintFileContents(string fileName)
        {
            var content = await DownloadFile(fileName);
            Console.WriteLine("File contents:");
            Console.WriteLine(content);
        }

        private async Task<string> DownloadFile(string fileName)
        {
            var url = await GetFileUrl(fileName);
            
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("Failed to download file");
                return string.Empty;
            }
             
            var content = await response.Content.ReadAsStringAsync();
            return content;
        }

        private async Task<string> GetFileUrl(string fileName)
        {
            //var model = await _db.Child(_folder).Child(fileName).GetDownloadUrlAsync();
            var model = await _db.Child(fileName).GetDownloadUrlAsync();
            return model;
        }
    }
}
