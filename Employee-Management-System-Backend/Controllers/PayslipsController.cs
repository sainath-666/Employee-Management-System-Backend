using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Employee_Management_System_Backend.Services;

namespace Employee_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PayslipsController : ControllerBase
    {
        private readonly IPayslipRepository _repo;
        private readonly PayslipUploadSettings _uploadSettings;
        private readonly IPdfService _pdfService;

        public PayslipsController(IPayslipRepository repo, IOptions<PayslipUploadSettings> uploadSettings, IPdfService pdfService)
        {
            _repo = repo;
            _uploadSettings = uploadSettings.Value;
            _pdfService = pdfService;
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

        // ✅ Generate and save payslip PDF for employee
        [HttpPost("generate/{employeeId}/{payslipId}")]
        public async Task<IActionResult> GeneratePayslipPdf(int employeeId, int payslipId)
        {
            try
            {
                var filePath = await _pdfService.GeneratePayslipPdfAsync(employeeId, payslipId);

                if (string.IsNullOrEmpty(filePath))
                    return BadRequest(new { message = "Failed to generate PDF" });

                return Ok(new
                {
                    message = "PDF generated successfully",
                    filePath,
                    fileName = Path.GetFileName(filePath)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generating PDF", error = ex.Message });
            }
        }

        // ✅ Return payslip PDF as downloadable file
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
    }
}
