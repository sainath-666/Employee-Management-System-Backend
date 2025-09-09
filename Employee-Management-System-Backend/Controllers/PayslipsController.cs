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

        // ✅ CORRECTED - UPDATE PAYSLIP + GENERATE PDF (Only updates audit fields and PDF path)
        [HttpPut("update-and-generate-pdf")]
        public async Task<IActionResult> UpdatePayslipAndGeneratePdf([FromBody] PayslipUpdateRequest request)
        {
            try
            {
                if (request.EmployeeId <= 0)
                    return BadRequest(new { message = "Valid Employee ID is required" });

                var employee = await _employeeRepository.GetEmployeeWithDepartmentAsync(request.EmployeeId);
                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                // Find the latest payslip for this employee
                var existingPayslip = await _repo.GetLatestPayslipByEmployeeIdAsync(request.EmployeeId);
                if (existingPayslip == null)
                    return NotFound(new { message = "No payslip found for this employee" });

                // ✅ ONLY update audit fields - DO NOT touch salary data
                existingPayslip.UpdatedDateTime = DateTime.UtcNow;
                existingPayslip.UpdatedBy = request.CreatedBy;

                // Update payslip in database (only audit fields)
                await _repo.UpdatePayslipAsync(existingPayslip);

                // Generate PDF and update PDF path
                var pdfPath = await _pdfService.GeneratePayslipPdfAsync(request.EmployeeId, existingPayslip.Id);
                var fileName = Path.GetFileName(pdfPath);
                var webCompatiblePath = pdfPath.Replace("\\", "/"); // ✅ FIXED escape sequence

                return Ok(new
                {
                    message = "PDF generated and path updated successfully",
                    payslipId = existingPayslip.Id,
                    pdfPath = webCompatiblePath,
                    fileName,
                    employeeName = employee.Name,
                    employeeCode = employee.EmployeeCode,
                    employeeEmail = employee.Email,
                    departmentName = employee.DepartmentName,
                    month = existingPayslip.Month,
                    baseSalary = existingPayslip.BaseSalary,  // ✅ Use existing values
                    allowances = existingPayslip.Allowances,   // ✅ Use existing values
                    deductions = existingPayslip.Deductions,   // ✅ Use existing values
                    netSalary = existingPayslip.NetSalary,     // ✅ Use existing values
                    downloadUrl = $"/api/payslips/download-file/{fileName}",
                    serveUrl = $"/api/payslips/serve/{fileName}"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }

        // ✅ CORRECTED - BULK UPDATE (Only updates audit fields and PDF path)
        [HttpPut("update-and-generate-pdf-bulk")]
        public async Task<IActionResult> UpdatePayslipsAndGeneratePdfsBulk([FromBody] BulkPayslipUpdateRequest request)
        {
            try
            {
                if (request.EmployeeIds == null || !request.EmployeeIds.Any())
                    return BadRequest(new { message = "At least one Employee ID is required" });

                var results = new List<object>();
                var errors = new List<object>();

                foreach (var employeeId in request.EmployeeIds)
                {
                    try
                    {
                        // 1. Check if employee exists
                        var employee = await _employeeRepository.GetEmployeeWithDepartmentAsync(employeeId);
                        if (employee == null)
                        {
                            errors.Add(new { employeeId, error = "Employee not found" });
                            continue;
                        }

                        // 2. Find the latest payslip for this employee
                        var existingPayslip = await _repo.GetLatestPayslipByEmployeeIdAsync(employeeId);
                        if (existingPayslip == null)
                        {
                            errors.Add(new { employeeId, error = "No payslip found for this employee" });
                            continue;
                        }

                        // ✅ ONLY update audit fields - DO NOT touch salary data
                        existingPayslip.UpdatedDateTime = DateTime.UtcNow;
                        existingPayslip.UpdatedBy = request.CreatedBy;

                        // Update payslip in database (only audit fields)
                        await _repo.UpdatePayslipAsync(existingPayslip);

                        // Generate PDF and save to disk + update PdfPath in database
                        var pdfPath = await _pdfService.GeneratePayslipPdfAsync(employeeId, existingPayslip.Id);
                        var fileName = Path.GetFileName(pdfPath);
                        var webCompatiblePath = pdfPath.Replace("\\", "/"); // ✅ FIXED escape sequence

                        results.Add(new
                        {
                            employeeId,
                            payslipId = existingPayslip.Id,
                            pdfPath = webCompatiblePath,
                            fileName,
                            employeeName = employee.Name,
                            employeeCode = employee.EmployeeCode,
                            employeeEmail = employee.Email,
                            departmentName = employee.DepartmentName,
                            month = existingPayslip.Month,
                            baseSalary = existingPayslip.BaseSalary,  // ✅ Use existing values
                            allowances = existingPayslip.Allowances,   // ✅ Use existing values  
                            deductions = existingPayslip.Deductions,   // ✅ Use existing values
                            netSalary = existingPayslip.NetSalary,     // ✅ Use existing values
                            updatedBy = request.CreatedBy,
                            downloadUrl = $"/api/payslips/download-file/{fileName}",
                            serveUrl = $"/api/payslips/serve/{fileName}"
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new { employeeId, error = ex.Message });
                    }
                }

                return Ok(new
                {
                    message = $"Bulk PDF generation completed. {results.Count} successful, {errors.Count} failed.",
                    totalRequested = request.EmployeeIds.Count,
                    successfulCount = results.Count,
                    errorCount = errors.Count,
                    results,
                    errors = errors.Any() ? errors : null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error in bulk PDF generation", error = ex.Message });
            }
        }


        // 🆕 CREATE CUSTOM PAYSLIP (Manual salary input, no PDF generation)
        [HttpPost("payslip-data")]
        public async Task<IActionResult> CreateCustomPayslip([FromBody] Payslip request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (request.EmployeeId <= 0)
                    return BadRequest(new { message = "Valid Employee ID is required" });

                // Validate employee exists
                var employee = await _employeeRepository.GetEmployeeWithDepartmentAsync(request.EmployeeId);
                if (employee == null)
                    return NotFound(new { message = "Employee not found" });

                // Set additional calculated/system fields
                request.NetSalary = request.BaseSalary + request.Allowances - request.Deductions;
                request.Status = true;
                request.CreatedDateTime = DateTime.UtcNow;
                // Month and CreatedBy should already be provided in request

                // Save to database
                var payslipId = await _repo.AddPayslipWithReturnIdAsync(request);

                return Ok(new
                {
                    message = "Custom payslip created successfully",
                    payslipId,
                    employeeName = employee.Name,
                    employeeCode = employee.EmployeeCode,
                    employeeEmail = employee.Email,
                    departmentName = employee.DepartmentName,
                    month = request.Month,
                    baseSalary = request.BaseSalary,
                    allowances = request.Allowances,
                    deductions = request.Deductions,
                    netSalary = request.NetSalary,
                    createdBy = request.CreatedBy
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating custom payslip", error = ex.Message });
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

        // Download the latest payslip PDF by employee ID
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
    }
}
