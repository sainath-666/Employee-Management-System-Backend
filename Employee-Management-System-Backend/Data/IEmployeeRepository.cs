using Employee_Management_System_Backend.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Data
{
    public interface IEmployeeRepository
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<int> CreateAsync(Employee employee);
        Task<int> UpdateAsync(Employee employee);
        Task<int> DeleteAsync(int id);

        // NEW: Method to get employee with department name
        Task<EmployeeWithDepartment?> GetEmployeeWithDepartmentAsync(int id);
    }
}
