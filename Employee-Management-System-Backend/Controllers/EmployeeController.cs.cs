using Employee_Management_System_Backend.Data;
using Employee_Management_System_Backend.Model;
using Employee_Management_System_Backend.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace Employee_Management_System_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly EmployeeRepository _repository;
        private readonly IWebHostEnvironment _env;

        public EmployeeController(EmployeeRepository repository, IWebHostEnvironment env)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        // GET: api/Employee
        [HttpGet]
        public ActionResult<List<Employee>> GetAll()
        {
            var employees = _repository.GetAll();
            return Ok(employees);
        }

        // GET: api/Employee/5
        [HttpGet("{id}")]
        public ActionResult<Employee> GetById(int id)
        {
            var employee = _repository.GetById(id);
            return employee == null ? NotFound($"Employee with Id = {id} not found.") : Ok(employee);
        }

        // POST: api/Employee
        [HttpPost]
        public async Task<ActionResult> Create([FromForm] EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string? filePath = await SaveProfilePhotoAsync(dto.ProfilePhoto);
            CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var employee = new Employee
            {
                EmployeeCode = dto.EmployeeCode,
                Name = dto.Name,
                Email = dto.Email,
                MobileNumber = dto.MobileNumber,
                Gender = dto.Gender,
                DOB = dto.DOB,
                ProfilePhotoPath = filePath,
                RoleId = dto.RoleId,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Status = true,
                CreatedDateTime = DateTime.Now
            };

            _repository.Create(employee);
            return Ok("Employee created successfully.");
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromForm] EmployeeUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = _repository.GetById(id);
            if (existing == null) return NotFound($"Employee with Id = {id} not found.");

            // Save new photo if provided, else keep existing
            string? filePath = dto.ProfilePhoto != null
                ? await SaveProfilePhotoAsync(dto.ProfilePhoto)
                : existing.ProfilePhotoPath;

            // Update fields safely
            existing.EmployeeCode = dto.EmployeeCode;
            existing.Name = dto.Name;
            existing.Email = dto.Email;
            existing.MobileNumber = dto.MobileNumber;
            existing.Gender = dto.Gender;
            existing.DOB = dto.DOB;
            existing.ProfilePhotoPath = filePath;
            existing.RoleId = dto.RoleId;
            existing.Status = dto.Status;
            existing.UpdatedDateTime = DateTime.Now;
            existing.UpdatedBy = dto.UpdatedBy;

            // Update password only if provided
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);
                existing.PasswordHash = passwordHash;
                existing.PasswordSalt = passwordSalt;
            }

            _repository.Update(existing);
            return Ok("Employee updated successfully.");
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var existing = _repository.GetById(id);
            if (existing == null) return NotFound($"Employee with Id = {id} not found.");

            _repository.Delete(id);
            return Ok("Employee deleted successfully.");
        }

        // --- Helpers ---
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = hmac.Key;
            passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private async Task<string?> SaveProfilePhotoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.GetTempPath(), "uploads", "employees");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var relativePath = Path.Combine("uploads", "employees", uniqueFileName);
            var fullPath = Path.Combine(_env.WebRootPath ?? Path.GetTempPath(), relativePath);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return relativePath.Replace("\\", "/");
        }
    }
}
