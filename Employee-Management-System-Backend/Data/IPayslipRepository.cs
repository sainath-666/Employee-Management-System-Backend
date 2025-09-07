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

        // NEW: Method to update PDF path after generation
        Task<int> UpdatePdfPathAsync(int payslipId, string pdfPath);

        // NEW: Method to create payslip and return the generated ID
        Task<int> AddPayslipWithReturnIdAsync(Payslip payslip);

        // NEW: Method to get payslip with employee details (fixes the "Name" column error)
        Task<PayslipWithEmployee?> GetPayslipWithEmployeeAsync(int payslipId);
    }
}
