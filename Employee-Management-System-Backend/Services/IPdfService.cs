using Employee_Management_System_Backend.Model;

namespace Employee_Management_System_Backend.Services
{
    public interface IPdfService
    {
        /// <summary>
        /// Generates a payslip PDF file and saves it on disk. 
        /// Returns the saved file path.
        /// </summary>
        Task<string> GeneratePayslipPdfAsync(int employeeId, int payslipId);

        /// <summary>
        /// Generates a payslip PDF and returns it as a byte array (for download).
        /// </summary>
        Task<byte[]> GeneratePayslipPdfBytesAsync(int employeeId, int payslipId);
    }
}
