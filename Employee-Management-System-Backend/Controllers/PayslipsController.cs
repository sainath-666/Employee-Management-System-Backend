using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;

namespace Employee_Management_System_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayslipsController : ControllerBase
    {
        private readonly PayslipRepository _repo;

        public PayslipsController(PayslipRepository repo)
        {
            _repo = repo;
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
    }
}
