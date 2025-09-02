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
        private readonly IEmployeeRepository _repository; // Use interface
        private readonly IWebHostEnvironment _env;

        public EmployeeController(IEmployeeRepository repository, IWebHostEnvironment env)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _env = env ?? throw new ArgumentNullException(nameof(env));
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetAll()
        {
            var employees = await _repository.GetAllAsync();
            return Ok(employees);
        }

        // GET: api/Employee/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetById(int id)
        {
            var employee = await _repository.GetByIdAsync(id);
            return employee == null ? NotFound($"Employee with Id = {id} not found.") : Ok(employee);
        }

        // POST: api/Employee
        [HttpPost]
        public async Task<ActionResult> Create([FromForm] EmployeeCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            string? filePath = await SaveProfilePhotoAsync(dto.ProfilePhoto);

            // Hash the password using BCrypt or similar secure method
            string hashedPassword = HashPassword(dto.Password);

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
                Password = hashedPassword, // Single hashed password field
                Status = true,
                CreatedDateTime = DateTime.Now
            };

            var newId = await _repository.CreateAsync(employee);
            return Ok(new { Message = "Employee created successfully.", Id = newId });
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromForm] EmployeeUpdateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _repository.GetByIdAsync(id);
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
                existing.Password = HashPassword(dto.Password); // Hash new password
            }

            var rowsAffected = await _repository.UpdateAsync(existing);
            return rowsAffected > 0
                ? Ok("Employee updated successfully.")
                : BadRequest("Failed to update employee.");
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound($"Employee with Id = {id} not found.");

            var rowsAffected = await _repository.DeleteAsync(id);
            return rowsAffected > 0
                ? Ok("Employee deleted successfully.")
                : BadRequest("Failed to delete employee.");
        }

        // --- Helpers ---

        /// <summary>
        /// Hash password using BCrypt (recommended) or similar secure method
        /// You'll need to install BCrypt.Net-Next NuGet package
        /// </summary>
        private static string HashPassword(string password)
        {
            // Option 1: Using BCrypt (RECOMMENDED - install BCrypt.Net-Next)
            // return BCrypt.Net.BCrypt.HashPassword(password);

            // Option 2: Simple SHA256 (less secure, for quick testing only)
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "YourSaltHere"));
            return Convert.ToBase64String(hashedBytes);
        }

        /// <summary>
        /// Verify password against stored hash
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            // Option 1: Using BCrypt (RECOMMENDED)
            // return BCrypt.Net.BCrypt.Verify(password, storedHash);

            // Option 2: Simple SHA256 verification (matches the HashPassword method above)
            string hashToCompare = HashPassword(password);
            return hashToCompare == storedHash;
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
