using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    }
}
