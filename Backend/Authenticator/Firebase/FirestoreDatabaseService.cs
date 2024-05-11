using Google.Cloud.Firestore;
using Authenticator.DataModels;
using System.Security.Cryptography;
using System.Text;

namespace Authenticator.Firebase
{
    public class FirestoreDatabaseService
    {
        private readonly string _path = AppDomain.CurrentDomain.BaseDirectory + @"federated-learning-platform-secrets.json";
        private const string _projectID = "federated-learning-platf-c15c0";
        private readonly FirestoreDb _db;

        private const string _collectionName = "Users";

        public FirestoreDatabaseService()
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", _path);
            _db = FirestoreDb.Create(_projectID);
        }

        public async Task<bool> AddUser(UserData userData)
        {
            DocumentReference documentReference = _db.Collection(_collectionName).Document(userData.email);

            var hashedPassword = HashPassword(userData.password);

            Dictionary<string, object> logItemDictionary = new Dictionary<string, object>
            {
                { "password", hashedPassword},
                { "role", userData.role }
            };

            try
            {
                await documentReference.SetAsync(logItemDictionary);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        public async Task<UserData?> GetUser(string email)
        {
            DocumentReference documentReference = _db.Collection(_collectionName).Document(email);
            DocumentSnapshot documentSnapshot = await documentReference.GetSnapshotAsync();

            var dict = documentSnapshot.ToDictionary();

            if (dict == null || dict["password"].ToString() == null || dict["role"].ToString() == null)
            {
                return null;
            }

            return new UserData(email, dict["password"].ToString(), dict["role"].ToString());
        }

        public async Task<bool> CheckUserPassword(string email, string password)
        {
            var user = await GetUser(email);

            if (user == null)
            {
                return false;
            }

            return CheckPasswordMatch(password, user.password);
        }

        public async Task<bool> DeleteUser(string email)
        {
            DocumentReference documentReference = _db.Collection(_collectionName).Document(email);
            try
            {
                await documentReference.DeleteAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }

        private string HashPassword(string password)
        {
            HashAlgorithm sha = SHA256.Create();
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }

        private bool CheckPasswordMatch(string password, string hashedPassword)
        {
            var hash = HashPassword(password);
            
            if (hash != hashedPassword)
            {
                Console.WriteLine("Hashed password: " + hash + " vs Hash provided: " + hashedPassword);
            }
            
            return hash == hashedPassword;
        }
    }
}
