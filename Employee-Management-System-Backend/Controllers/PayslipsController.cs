using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Employee_Management_System_Backend.Services;
using System.Text.Json;

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

        // ✅ Add payslip
        [HttpPost]
        public async Task<IActionResult> Add(Payslip ps)
        {
            ps.CreatedDateTime = DateTime.UtcNow;
            var rows = await _repo.AddPayslipAsync(ps);
            if (rows > 0)
                return Ok(new { message = "Payslip created successfully" });
            return BadRequest(new { message = "Failed to create payslip" });
        }

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

        // ✅ Generate and save payslip PDF for employee (existing payslip)
        [HttpPost("generate/{employeeId}/{payslipId}")]
        public async Task<IActionResult> GeneratePayslipPdf(int employeeId, int payslipId)
        {
            try
            {
                var filePath = await _pdfService.GeneratePayslipPdfAsync(employeeId, payslipId);
                if (string.IsNullOrEmpty(filePath))
                    return BadRequest(new { message = "Failed to generate PDF" });
                var fileName = Path.GetFileName(filePath);
                return Ok(new
                {
                    message = "PDF generated successfully",
                    pdfPath = filePath.Replace("\\", "/"),
                    fileName,
                    downloadUrl = $"/api/payslips/download-file/{fileName}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }

        // ✅ Return payslip PDF as downloadable file (existing payslip)
        [HttpGet("download/{employeeId}/{payslipId}")]
        public async Task<IActionResult> DownloadPayslipPdf(int employeeId, int payslipId)
        {
            try
            {
                var pdfBytes = await _pdfService.GeneratePayslipPdfBytesAsync(employeeId, payslipId);
                if (pdfBytes == null || pdfBytes.Length == 0)
                    return NotFound(new { message = "Payslip not found" });
                return File(pdfBytes, "application/pdf", $"Payslip_{employeeId}_{payslipId}.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error downloading PDF", error = ex.Message });
            }
        }

        // ✅ MAIN ENDPOINT - CREATE PAYSLIP + GENERATE PDF with employee data fetching
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

        // ✅ Generate PDF from data (without DB insert) - returns relative path
        [HttpPost("generate-from-data")]
        public async Task<IActionResult> GeneratePayslipFromData([FromBody] PayslipRequest request)
        {
            try
            {
                if (request.EmployeeId <= 0)
                    return BadRequest(new { message = "Valid Employee ID is required" });
                var relativePdfPath = await _pdfService.GeneratePayslipFromRequestAsync(request);
                if (string.IsNullOrEmpty(relativePdfPath))
                    return BadRequest(new { message = "Failed to generate PDF" });
                var fileName = Path.GetFileName(relativePdfPath);
                var webCompatiblePath = relativePdfPath.Replace("\\", "/");
                return Ok(new
                {
                    message = "PDF generated successfully",
                    pdfPath = webCompatiblePath,
                    fileName,
                    downloadUrl = $"/api/payslips/download-file/{fileName}",
                    serveUrl = $"/api/payslips/serve/{fileName}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }

        // ✅ Download PDF directly from PayslipRequest data
        [HttpPost("download-from-data")]
        public async Task<IActionResult> DownloadPayslipFromData([FromBody] PayslipRequest request)
        {
            try
            {
                if (request.EmployeeId <= 0)
                    return BadRequest(new { message = "Valid Employee ID is required" });
                var pdfBytes = await _pdfService.GeneratePayslipBytesFromRequestAsync(request);
                if (pdfBytes == null || pdfBytes.Length == 0)
                    return BadRequest(new { message = "Failed to generate PDF" });
                var fileName = $"Payslip_{request.EmployeeId}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }

        // ✅ Generate PDF from JSON with proper path storage
        [HttpPost("generate-from-json")]
        public async Task<IActionResult> GeneratePayslipFromJson([FromBody] JsonElement jsonData)
        {
            try
            {
                var jsonString = jsonData.GetRawText();
                if (string.IsNullOrEmpty(jsonString))
                    return BadRequest(new { message = "JSON data is required" });
                var relativePdfPath = await _pdfService.GeneratePayslipFromJsonAsync(jsonString);
                if (string.IsNullOrEmpty(relativePdfPath))
                    return BadRequest(new { message = "Failed to generate PDF from JSON" });
                var fileName = Path.GetFileName(relativePdfPath);
                var webCompatiblePath = relativePdfPath.Replace("\\", "/");
                return Ok(new
                {
                    message = "PDF generated successfully from JSON",
                    pdfPath = webCompatiblePath,
                    fileName,
                    downloadUrl = $"/api/payslips/download-file/{fileName}",
                    serveUrl = $"/api/payslips/serve/{fileName}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF from JSON", error = ex.Message });
            }
        }

        // ✅ Download PDF directly from JSON string
        [HttpPost("download-from-json")]
        public async Task<IActionResult> DownloadPayslipFromJson([FromBody] JsonElement jsonData)
        {
            try
            {
                var jsonString = jsonData.GetRawText();
                if (string.IsNullOrEmpty(jsonString))
                    return BadRequest(new { message = "JSON data is required" });
                var pdfBytes = await _pdfService.GeneratePayslipBytesFromJsonAsync(jsonString);
                if (pdfBytes == null || pdfBytes.Length == 0)
                    return BadRequest(new { message = "Failed to generate PDF from JSON" });
                var fileName = $"Payslip_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF from JSON", error = ex.Message });
            }
        }

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

        // ✅ Serve PDF files securely (controlled access)
        [HttpGet("serve/{fileName}")]
        public IActionResult ServePdf(string fileName)
        {
            try
            {
                var filePath = Path.Combine("Uploads", "Payslips", fileName);
                if (!System.IO.File.Exists(filePath))
                    return NotFound(new { message = "PDF file not found" });
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error serving PDF", error = ex.Message });
            }
        }
    }
}
