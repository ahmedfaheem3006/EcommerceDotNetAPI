using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ECommerce.API.Helpers;
using ECommerce.BLL.Interfaces;

namespace ECommerce.API.Controllers
{
    /// <summary>
    /// AuthController: Handles User Registration & Login
    /// 
    /// Endpoints:
    /// POST /api/auth/register - Create a new user account
    /// POST /api/auth/login    - Authenticate and get a JWT token
    /// </summary>
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return HandleResult(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);
            return HandleResult(result);
        }
    }
}
