using Microsoft.AspNetCore.Mvc;
using RestApi.DTOS;
using RestApi.HttpClients;

namespace RestApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticatorService _authenticatorService;
        private readonly ILoggerService _loggerService;
        public AuthenticationController(IAuthenticatorService authenticatorService, ILoggerService loggerService)
        {
            _authenticatorService = authenticatorService;
            _loggerService = loggerService;
        }

        [HttpPost("registerUser")]
        public async Task<IActionResult> RegisterUser([FromBody] UserData userData)
        {
            var r = await _authenticatorService.RegisterUser(userData);
            if (!r)
            {
                return BadRequest();
            }

            return Ok();
        }

        [HttpPost("generateJWT")]
        public async Task<IActionResult> GenerateJWT([FromBody] UserData userData)
        {
            var jwt = await _authenticatorService.GenerateJWT(userData);
            if (jwt == null)
            {
                return Unauthorized();
            }

            var userToken = new UserToken(jwt);
            return Ok(userToken.token);
        }

        [HttpDelete("deleteUser/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var r = await _authenticatorService.DeleteUser(email);
            if (!r)
            {
                return BadRequest();
            }

            return Ok();
        }

    }
}
