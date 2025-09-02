using Employee_Management_System_Backend.DTOs;
using Employee_Management_System_Backend.Model;

namespace Employee_Management_System_Backend.Services
{
    public interface IAuthService
    {
        Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
        string GenerateJwtToken(Employee employee);
        bool VerifyPassword(string password, string storedHash);
        string HashPassword(string password);
    }
}
