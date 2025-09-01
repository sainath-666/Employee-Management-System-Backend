using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentController : ControllerBase
    {
        private readonly DepartmentRepository _repo;

        public DepartmentController(DepartmentRepository repo)
        {
            _repo = repo;
        }

        // GET: api/department
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Department>>> GetAllDepartments()
        {
            var departments = await _repo.GetAllAsync();
            return Ok(departments);
        }

        // GET: api/department/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Department>> GetDepartment(int id)
        {
            var dept = await _repo.GetByIdAsync(id);
            if (dept == null) return NotFound();
            return Ok(dept);
        }

        // POST: api/department
        [HttpPost]
        public async Task<ActionResult> AddDepartment([FromBody] Department dept)
        {
            await _repo.AddAsync(dept);
            return CreatedAtAction(nameof(GetDepartment), new { id = dept.Id }, dept);
        }

        // PUT: api/department/5
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateDepartment(int id, [FromBody] Department dept)
        {
            if (id != dept.Id) return BadRequest("ID mismatch");

            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _repo.UpdateAsync(dept);
            return NoContent();
        }

        // DELETE: api/department/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDepartment(int id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return NotFound();

            await _repo.DeleteAsync(id);
            return NoContent();
        }
    }
}
