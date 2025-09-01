using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Microsoft.Extensions.Options;

namespace Employee_Management_System_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayslipsController : ControllerBase
    {
        private readonly PayslipRepository _repo;
        private readonly PayslipUploadSettings _uploadSettings;

        public PayslipsController(PayslipRepository repo, IOptions<PayslipUploadSettings> uploadSettings)
        {
            _repo = repo;
            _uploadSettings = uploadSettings.Value;
        }

        // GET: api/payslips
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var payslips = await _repo.GetPayslipsAsync();
            return Ok(payslips);
        }

        // GET: api/payslips/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var ps = await _repo.GetPayslipByIdAsync(id);
            if (ps == null) return NotFound(new { message = "Payslip not found" });
            return Ok(ps);
        }

        // POST: api/payslips
        [HttpPost]
        public async Task<IActionResult> Add(Payslip ps)
        {
            var rows = await _repo.AddPayslipAsync(ps);
            if (rows > 0)
                return Ok(new { message = "Payslip created successfully" });
            return BadRequest(new { message = "Failed to create payslip" });
        }

        // PUT: api/payslips/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Payslip ps)
        {
            ps.Id = id;
            var rows = await _repo.UpdatePayslipAsync(ps);
            if (rows > 0)
                return Ok(new { message = "Payslip updated successfully" });
            return NotFound(new { message = "Payslip not found or update failed" });
        }

        // DELETE: api/payslips/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rows = await _repo.DeletePayslipAsync(id);
            if (rows > 0)
                return Ok(new { message = "Payslip deleted successfully" });
            return NotFound(new { message = "Payslip not found" });
        }

        // POST: api/payslips/{id}/upload-pdf
        [HttpPost("{id}/upload-pdf")]
        public async Task<IActionResult> UploadPdf(int id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file uploaded" });

            if (!file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { message = "Only PDF files are allowed" });

            var payslip = await _repo.GetPayslipByIdAsync(id);
            if (payslip == null)
                return NotFound(new { message = "Payslip not found" });

            // Generate a unique filename
            var fileName = $"payslip_{id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            var filePath = Path.Combine(_uploadSettings.UploadsPath, fileName);

            // Delete existing file if it exists
            if (!string.IsNullOrEmpty(payslip.PdfPath) && System.IO.File.Exists(payslip.PdfPath))
            {
                System.IO.File.Delete(payslip.PdfPath);
            }

            // Save the new file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Update the payslip with the new file path
            payslip.PdfPath = filePath;
            await _repo.UpdatePayslipAsync(payslip);

            return Ok(new { message = "PDF uploaded successfully" });
        }

        // GET: api/payslips/{id}/download-pdf
        [HttpGet("{id}/download-pdf")]
        public async Task<IActionResult> DownloadPdf(int id)
        {
            var payslip = await _repo.GetPayslipByIdAsync(id);
            if (payslip == null)
                return NotFound(new { message = "Payslip not found" });

            if (string.IsNullOrEmpty(payslip.PdfPath) || !System.IO.File.Exists(payslip.PdfPath))
                return NotFound(new { message = "PDF file not found" });

            var fileStream = new FileStream(payslip.PdfPath, FileMode.Open, FileAccess.Read);
            return File(fileStream, "application/pdf", Path.GetFileName(payslip.PdfPath));
        }
    }
}
