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

        /// <summary>
        /// Generates payslip PDF from frontend request data and saves it on disk.
        /// Returns the saved file path.
        /// </summary>
        Task<string> GeneratePayslipFromRequestAsync(PayslipRequest request);

        /// <summary>
        /// Generates payslip PDF from frontend request data and returns it as byte array.
        /// </summary>
        Task<byte[]> GeneratePayslipBytesFromRequestAsync(PayslipRequest request);

        /// <summary>
        /// Converts JSON string to PDF and saves it on disk.
        /// Returns the saved file path.
        /// </summary>
        Task<string> GeneratePayslipFromJsonAsync(string jsonData);

        /// <summary>
        /// Converts JSON string to PDF and returns it as byte array.
        /// </summary>
        Task<byte[]> GeneratePayslipBytesFromJsonAsync(string jsonData);

        /// <summary>
        /// Generates payslip PDFs in bulk for multiple employee IDs.
        /// Returns dictionary mapping employeeId to saved PDF path.
        /// </summary>
        Task<Dictionary<int, string>> GeneratePayslipsPdfForMultipleAsync(List<int> employeeIds, int createdBy);
    }
}
