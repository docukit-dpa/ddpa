using Microsoft.Extensions.Logging;

namespace DDPA.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        protected readonly ILogger _logger;

        public AuthenticationService(ILogger<AuthenticationService> logger)
        {
            _logger = logger;
        }
    }
}
