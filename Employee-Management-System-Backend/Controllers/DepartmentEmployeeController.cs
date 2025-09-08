using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentEmployeeController : ControllerBase
    {
        private readonly DepartmentEmployeeRepository _repo;

        public DepartmentEmployeeController(DepartmentEmployeeRepository repo)
        {
            _repo = repo;
        }

        // GET: api/departmentemployee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentEmployee>>> GetAllDepartmentEmployees()
        {
            var deptEmps = await _repo.GetAllDepartmentEmployeesAsync();
            return Ok(deptEmps);
        }

        // POST: api/departmentemployee
        // Assign employee to one or multiple departments
        [HttpPost]
        public async Task<ActionResult> AssignDepartments([FromBody] DepartmentEmployeeRequest request)
        {
            if (request.DepartmentIds == null || request.DepartmentIds.Count == 0)
                return BadRequest("At least one department must be assigned");

            await _repo.AssignEmployeeToDepartmentsAsync(request.EmployeeId, request.DepartmentIds);
            return Ok("Departments assigned successfully");
        }

        // GET: api/departmentemployee/5
        // Get all departments for an employee
        [HttpGet("{employeeId}")]
        public async Task<ActionResult<IEnumerable<Department>>> GetDepartmentsForEmployee(int employeeId)
        {
            var departments = await _repo.GetDepartmentsByEmployeeAsync(employeeId);
            return Ok(departments);
        }

        // DELETE: api/departmentemployee/5/3
        // Remove a department from employee
        [HttpDelete("{employeeId}/{departmentId}")]
        public async Task<ActionResult> RemoveDepartmentFromEmployee(int employeeId, int departmentId)
        {
            await _repo.RemoveDepartmentFromEmployeeAsync(employeeId, departmentId);
            return NoContent();
        }
    }

    // Request model for assigning multiple departments
    public class DepartmentEmployeeRequest
    {
        public int EmployeeId { get; set; }
        public List<int> DepartmentIds { get; set; } = new();
    }
}
