using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeaveController : ControllerBase
    {
        private readonly LeaveRepository _repo;

        public LeaveController(LeaveRepository repository)
        {

            _repo = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _repo.GetAllLeave());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var leave = await _repo.GetLeaveById(id);
            if (leave == null) return NotFound();
            return Ok(leave);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Leave leave)
        {
            await _repo.CreateLeave(leave);
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Leave leave)
        {
            leave.Id = id;
            await _repo.UpdateLeave(leave);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteLeave(id);
            return Ok();
        }
    }
}
