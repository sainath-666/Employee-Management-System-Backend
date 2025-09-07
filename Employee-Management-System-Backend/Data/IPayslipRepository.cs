using Employee_Management_System_Backend.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Data
{
    public interface IPayslipRepository
    {
        Task<IEnumerable<Payslip>> GetPayslipsAsync();
        Task<Payslip?> GetPayslipByIdAsync(int id);
        Task<int> AddPayslipAsync(Payslip payslip);
        Task<int> UpdatePayslipAsync(Payslip payslip);
        Task<int> DeletePayslipAsync(int id);
    }
}
