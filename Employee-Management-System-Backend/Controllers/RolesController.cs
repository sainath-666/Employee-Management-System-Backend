using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly RoleRepository _repository;

        public RolesController(RoleRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _repository.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = await _repository.GetRoleByIdAsync(id);
            if (role == null) return NotFound();
            return Ok(role);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Role role)
        {
            // Set the required fields
            role.CreatedBy = null;
            role.UpdatedBy = null;
            role.CreatedDateTime = DateTime.UtcNow;
            role.UpdatedDateTime = null;
            
            var newId = await _repository.AddRoleAsync(role);
            role.Id = newId;
            return CreatedAtAction(nameof(GetById), new { id = newId }, role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Role role)
        {
            if (id != role.Id) return BadRequest();

            // Get the existing role to preserve CreatedBy and CreatedDateTime
            var existingRole = await _repository.GetRoleByIdAsync(id);
            if (existingRole == null) return NotFound();

            // Update only the allowed fields
            role.CreatedBy = existingRole.CreatedBy;
            role.CreatedDateTime = existingRole.CreatedDateTime;
            role.UpdatedBy = null;
            role.UpdatedDateTime = DateTime.UtcNow;

            bool updated = await _repository.UpdateRoleAsync(role);
            if (!updated) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _repository.DeleteRoleAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
