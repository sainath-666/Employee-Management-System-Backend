using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Employee_Management_System_Backend.Services;
using System.Text.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayslipsController : ControllerBase
    {
        private readonly IPayslipRepository _repo;
        private readonly PayslipUploadSettings _uploadSettings;
        private readonly IPdfService _pdfService;
        private readonly IEmployeeRepository _employeeRepository;

        public PayslipsController(IPayslipRepository repo, IOptions<PayslipUploadSettings> uploadSettings, IPdfService pdfService, IEmployeeRepository employeeRepository)
        {
            _repo = repo;
            _uploadSettings = uploadSettings.Value;
            _pdfService = pdfService;
            _employeeRepository = employeeRepository;
        }

        // ✅ Get all payslips
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payslips = await _repo.GetPayslipsAsync();
            return Ok(payslips);
        }

        // ✅ Get payslip by ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ps = await _repo.GetPayslipByIdAsync(id);
            if (ps == null) return NotFound(new { message = "Payslip not found" });
            return Ok(ps);
        }

        // Add payslip endpoint commented out
        /*
        [HttpPost]
        public async Task<IActionResult> Add(Payslip ps)
        {
            ps.CreatedDateTime = DateTime.UtcNow;
            var rows = await _repo.AddPayslipAsync(ps);
            if (rows > 0)
                return Ok(new { message = "Payslip created successfully" });
            return BadRequest(new { message = "Failed to create payslip" });
        }
        */

        // ✅ Update payslip
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Payslip ps)
        {
            ps.Id = id;
            ps.UpdatedDateTime = DateTime.UtcNow;
            var rows = await _repo.UpdatePayslipAsync(ps);
            if (rows > 0)
                return Ok(new { message = "Payslip updated successfully" });
            return NotFound(new { message = "Payslip not found or update failed" });
        }

        // ✅ Delete payslip
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rows = await _repo.DeletePayslipAsync(id);
            if (rows > 0)
                return Ok(new { message = "Payslip deleted successfully" });
            return NotFound(new { message = "Payslip not found" });
        }

        // PDF generation endpoint commented out
        /*
        [HttpPost("generate/{employeeId}/{payslipId}")]
        public async Task<IActionResult> GeneratePayslipPdf(int employeeId, int payslipId)
        {
            // Implementation
        }
        */

        // PDF download endpoint commented out
        /*
        [HttpGet("download/{employeeId}/{payslipId}")]
        public async Task<IActionResult> DownloadPayslipPdf(int employeeId, int payslipId)
        {
            // Implementation
        }
        */

        // ✅ MAIN ENDPOINT - CREATE PAYSLIP + GENERATE PDF
        [HttpPost("create-and-generate-pdf")]
        public async Task<IActionResult> CreatePayslipAndGeneratePdf([FromBody] PayslipRequest request)
        {
            try
            {
                if (request.EmployeeId <= 0)
                    return BadRequest(new { message = "Valid Employee ID is required" });
                var employee = await _employeeRepository.GetEmployeeWithDepartmentAsync(request.EmployeeId);
                if (employee == null)
                    return NotFound(new { message = "Employee not found" });
                var payslip = new Payslip
                {
                    EmployeeId = request.EmployeeId,
                    Salary = request.BaseSalary + request.Allowances,
                    BaseSalary = request.BaseSalary,
                    Allowances = request.Allowances,
                    Deductions = request.Deductions,
                    Month = DateTime.Now.ToString("MMMM yyyy"),
                    Status = true,
                    CreatedDateTime = DateTime.UtcNow,
                    CreatedBy = request.CreatedBy
                };
                var payslipId = await _repo.AddPayslipWithReturnIdAsync(payslip);
                var pdfPath = await _pdfService.GeneratePayslipPdfAsync(request.EmployeeId, payslipId);
                var fileName = Path.GetFileName(pdfPath);
                var webCompatiblePath = pdfPath.Replace("\\", "/");
                return Ok(new
                {
                    message = "Payslip created and PDF generated successfully",
                    payslipId,
                    pdfPath = webCompatiblePath,
                    fileName,
                    employeeName = employee.Name,
                    employeeCode = employee.EmployeeCode,
                    employeeEmail = employee.Email,
                    departmentName = employee.DepartmentName,
                    month = payslip.Month,
                    downloadUrl = $"/api/payslips/download-file/{fileName}",
                    serveUrl = $"/api/payslips/serve/{fileName}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating payslip and generating PDF", error = ex.Message });
            }
        }

        // Generate-from-data endpoint commented out
        /*
        [HttpPost("generate-from-data")]
        public async Task<IActionResult> GeneratePayslipFromData([FromBody] PayslipRequest request)
        {
            // Implementation
        }
        */

        // Download-from-data endpoint commented out
        /*
        [HttpPost("download-from-data")]
        public async Task<IActionResult> DownloadPayslipFromData([FromBody] PayslipRequest request)
        {
            // Implementation
        }
        */

        // Generate-from-json endpoint commented out
        /*
        [HttpPost("generate-from-json")]
        public async Task<IActionResult> GeneratePayslipFromJson([FromBody] JsonElement jsonData)
        {
            // Implementation
        }
        */

        // Download-from-json endpoint commented out
        /*
        [HttpPost("download-from-json")]
        public async Task<IActionResult> DownloadPayslipFromJson([FromBody] JsonElement jsonData)
        {
            // Implementation
        }
        */

        // ✅ Download saved PDF file by filename
        [HttpGet("download-file/{fileName}")]
        public IActionResult DownloadSavedPdf(string fileName)
        {
            try
            {
                var filePath = Path.Combine("Uploads", "Payslips", fileName);
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "PDF file not found" });
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error downloading PDF", error = ex.Message });
            }
        }

        // New endpoint to download the latest payslip PDF by employee ID
        [HttpGet("download-latest/{employeeId}")]
        public async Task<IActionResult> DownloadLatestPayslipByEmployee(int employeeId)
        {
            var payslips = await _repo.GetPayslipsByEmployeeIdAsync(employeeId);
            var latestPayslip = payslips?.OrderByDescending(p => p.CreatedDateTime).FirstOrDefault();

            if (latestPayslip == null || string.IsNullOrEmpty(latestPayslip.PdfPath))
                return NotFound(new { message = "Payslip PDF not found for employee" });

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), latestPayslip.PdfPath);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "Payslip PDF file does not exist on the server" });

            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);

            return File(fileBytes, "application/pdf", fileName);
        }

        // Serve PDF files endpoint commented out
        /*
        [HttpGet("serve/{fileName}")]
        public IActionResult ServePdf(string fileName)
        {
            // Implementation
        }
        */
    }
}
