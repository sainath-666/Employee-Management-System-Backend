using Employee_Management_System_Backend.Model;

namespace Employee_Management_System_Backend.Data
{
    public interface IAuthRepository
    {
        Task<Employee?> GetEmployeeByEmailAsync(string email);
        Task<bool> UpdateLastLoginAsync(int employeeId, DateTime loginTime);
        Task<Employee?> GetEmployeeByIdAsync(int id);
    }
}
