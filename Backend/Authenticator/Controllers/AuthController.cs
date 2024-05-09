using Microsoft.AspNetCore.Mvc;
using Authenticator.Auth;
using Authenticator.DataModels;

namespace Authenticator.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("registerUser")]
        public async Task<IActionResult> RegisterUser([FromBody] UserData userData)
        {
            var r = await _authService.RegisterUser(userData);
            if (!r)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("generateToken")]
        public async Task<IActionResult> GenerateToken([FromBody] UserData userData)
        {
            var jwt = await _authService.GenerateUserToken(userData);
            if (jwt == null)
            {
                return Unauthorized();
            }

            var userToken = new UserToken(jwt);
            return Ok(userToken);
        }

        [HttpDelete("deleteUser/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var r = await _authService.DeleteUser(email);
            if (!r)
            {
                return BadRequest();
            }

            return Ok();
        }

    }
}
