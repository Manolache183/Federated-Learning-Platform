using Authenticator.DTOS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Authenticator.Controllers
{
    [ApiController]
    public class IdentityController : ControllerBase
    {
        private const string SecretKey = "ThisIsTheSecretKeyThisIsTheSecretKey";
        private readonly TimeSpan TokenExpiration = TimeSpan.FromMinutes(30);

        [HttpPost("getJwtToken")]
        public IActionResult GetJwtToken([FromBody] TokenGenerationRequest tokenRequest)
        {
            Console.WriteLine("Received token request: ", tokenRequest);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(SecretKey);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, tokenRequest.Email),
                new Claim(JwtRegisteredClaimNames.Sub, tokenRequest.Email)
            };

            foreach (var claimPair in tokenRequest.Claims)
            {
                var jsonElement = claimPair.Value;
                var valueType = jsonElement.ValueKind switch
                {
                    JsonValueKind.Number => ClaimValueTypes.Double,
                    JsonValueKind.True => ClaimValueTypes.Boolean,
                    JsonValueKind.False => ClaimValueTypes.Boolean,
                    _ => ClaimValueTypes.String
                };

                var claim = new Claim(claimPair.Key, claimPair.Value.ToString(), valueType);
                claims.Add(claim);
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.Add(TokenExpiration),
                Issuer = "Authenticator",
                Audience = "licenta",
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            Console.WriteLine("Received token: ", jwt);
            return Ok(jwt);
        }
    }
}
