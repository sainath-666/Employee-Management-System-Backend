using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.DTOs;
using Employee_Management_System_Backend.Model;
using Employee_Management_System_Backend.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Employee_Management_System_Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IEmployeeRepository _repository;
        private readonly IConfiguration _configuration;

        public AuthService(IEmployeeRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
        {
            Console.WriteLine($"🔍 Login attempt for: {loginDto.Email}");

            // Get all employees and find by email
            var employees = await _repository.GetAllAsync();
            var employee = employees.FirstOrDefault(e =>
                e.Email.Equals(loginDto.Email, StringComparison.OrdinalIgnoreCase));

            if (employee == null)
            {
                Console.WriteLine("❌ Employee not found");
                return null;
            }

            Console.WriteLine($"✅ Employee found: {employee.Name}");
            Console.WriteLine($"📊 Status: {employee.Status}");
            Console.WriteLine($"🔑 Stored password: '{employee.Password}'");
            Console.WriteLine($"🔑 Input password: '{loginDto.Password}'");

            // Verify password with hybrid approach
            if (!VerifyPassword(loginDto.Password, employee.Password))
            {
                Console.WriteLine("❌ Password verification failed");
                return null;
            }

            // Check if employee is active
            if (!employee.Status)
            {
                Console.WriteLine("❌ Account is disabled");
                return null;
            }

            Console.WriteLine("✅ Login successful - generating token");

            // Generate JWT token
            var token = GenerateJwtToken(employee);

            return new LoginResponseDto
            {
                Token = token,
                Message = "Login successful",
                EmployeeId = employee.Id,
                Name = employee.Name,
                Email = employee.Email
            };
        }

        public string GenerateJwtToken(Employee employee)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "YourSecretKeyHere");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, employee.Id.ToString()),
                    new Claim(ClaimTypes.Email, employee.Email),
                    new Claim(ClaimTypes.Name, employee.Name),
                    new Claim("RoleId", employee.RoleId.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(8), // 8 hour expiry
                Issuer = _configuration["Jwt:Issuer"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public bool VerifyPassword(string password, string storedHash)
        {
            // HYBRID APPROACH: Handle both plain text and hashed passwords

            // Check if stored password looks like a hash (hashes are long, typically 40+ chars)
            if (storedHash.Length < 20)
            {
                // Plain text password comparison (for existing users like your "sai123")
                Console.WriteLine("🔍 Using plain text comparison");
                return password == storedHash;
            }
            else
            {
                // Hashed password comparison (for new users created via EmployeeController)
                Console.WriteLine("🔍 Using hash comparison");
                using var sha256 = SHA256.Create();
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "YourSaltHere"));
                string hashToCompare = Convert.ToBase64String(hashedBytes);

                bool result = hashToCompare == storedHash;
                Console.WriteLine($"🔐 Hash comparison result: {result}");
                return result;
            }
        }

        public string HashPassword(string password)
        {
            // Use same hashing method as in EmployeeController
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "YourSaltHere"));
            return Convert.ToBase64String(hashedBytes);
        }
    }
}
