using Authenticator.Firebase;
using Authenticator.DataModels;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authenticator.Auth
{
    public class AuthService
    {
        private readonly FirestoreDatabaseService _firestoreDatabaseService;

        private const string _secretKey = "ThisIsTheSecretKeyThisIsTheSecretKey";
        private readonly TimeSpan _tokenExpiration = TimeSpan.FromMinutes(30);

        public AuthService()
        {
            _firestoreDatabaseService = new FirestoreDatabaseService();
        }

        public async Task<bool> RegisterUser(UserData userData) => await _firestoreDatabaseService.AddUser(userData);

        public async Task<UserData?> GetUser(string email) => await _firestoreDatabaseService.GetUser(email);

        public async Task<bool> DeleteUser(string email) => await _firestoreDatabaseService.DeleteUser(email);

        public async Task<string?> GenerateUserToken(UserData userData)
        {
            var r = await AuthenticateUser(userData);
            if (!r)
            {
                return null;
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            Console.WriteLine("Generating token for user: " + userData.email + " " + userData.password + " " + userData.role);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, userData.email),
                new Claim(JwtRegisteredClaimNames.Sub, userData.email),
                new Claim(ClaimTypes.Role, userData.role)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(_tokenExpiration),
                Issuer = "Authenticator",
                Audience = "licenta",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return jwt;
        }

        private async Task<bool> AuthenticateUser(UserData userData) => await _firestoreDatabaseService.CheckUserPassword(userData.email, userData.password);
    }
}
