using Employee_Management_System_Backend.DTOs;
using Employee_Management_System_Backend.Services;
using Microsoft.AspNetCore.Authorization; // ADD THIS
using Microsoft.AspNetCore.Mvc;

namespace Employee_Management_System_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous] // ADD THIS - Override global auth requirement
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        [AllowAnonymous] // ADD THIS - Allow anonymous access to login
        public async Task<ActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(loginDto);

                if (result == null)
                    return Unauthorized("Invalid email, password, or account is disabled.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
