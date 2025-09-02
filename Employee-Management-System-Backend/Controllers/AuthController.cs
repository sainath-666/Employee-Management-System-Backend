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

        // POST: api/Auth/validate-token (This should require authentication)
        [HttpPost("validate-token")]
        [Authorize] // ADD THIS - This endpoint should require authentication
        public ActionResult ValidateToken()
        {
            // This endpoint can be used by frontend to check if user is still authenticated
            // The actual validation will be done by JWT middleware
            return Ok(new { Message = "Token is valid", IsValid = true });
        }

        // POST: api/Auth/logout (This should require authentication)
        [HttpPost("logout")]
        [Authorize] // ADD THIS - This endpoint should require authentication
        public ActionResult Logout()
        {
            // Since JWT is stateless, logout is handled on the client side
            // by removing the token from storage
            return Ok(new { Message = "Logout successful" });
        }
    }
}
