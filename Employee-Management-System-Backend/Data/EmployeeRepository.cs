using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using Employee_Management_System_Backend.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Employee_Management_System_Backend.Data
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;
        private readonly string _uploadsPath;

        public EmployeeRepository(IConfiguration configuration, IOptions<EmployeeUploadSettings> uploadSettings)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new ArgumentNullException(nameof(configuration), "Connection string 'DefaultConnection' is missing.");
            _uploadsPath = uploadSettings.Value.UploadsPath;
        }

        // CORRECTED: Get employee with department name via proper three-table JOIN
        public async Task<EmployeeWithDepartment?> GetEmployeeWithDepartmentAsync(int id)
        {
            var query = @"
                SELECT 
                    e.Id, e.EmployeeCode, e.Name, e.Email, e.MobileNumber, 
                    e.Gender, e.DOB, e.ProfilePhotoPath, e.RoleId,
                    e.Status, e.CreatedDateTime,
                    d.Id AS DepartmentId,
                    d.DepartmentName AS DepartmentName
                FROM Employees e
                LEFT JOIN DepartmentEmployees de ON e.Id = de.EmployeeId
                LEFT JOIN Departments d ON de.DepartmentId = d.Id
                WHERE e.Id = @Id";

            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Id", id);

            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new EmployeeWithDepartment
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    EmployeeCode = reader.GetString(reader.GetOrdinal("EmployeeCode")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    MobileNumber = reader.GetString(reader.GetOrdinal("MobileNumber")),
                    Gender = reader.GetString(reader.GetOrdinal("Gender")),
                    DOB = reader.IsDBNull(reader.GetOrdinal("DOB")) ? null : reader.GetDateTime(reader.GetOrdinal("DOB")),
                    ProfilePhotoPath = reader.IsDBNull(reader.GetOrdinal("ProfilePhotoPath")) ? null : reader.GetString(reader.GetOrdinal("ProfilePhotoPath")),
                    RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                    DepartmentId = reader.IsDBNull(reader.GetOrdinal("DepartmentId")) ? null : reader.GetInt32(reader.GetOrdinal("DepartmentId")),
                    DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? null : reader.GetString(reader.GetOrdinal("DepartmentName")),
                    Status = reader.GetBoolean(reader.GetOrdinal("Status")),
                    CreatedDateTime = reader.GetDateTime(reader.GetOrdinal("CreatedDateTime"))
                };
            }

            return null;
        }

        // FIXED: Create employee without DepartmentId
        public async Task<int> CreateAsync(Employee employee)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(@"
                INSERT INTO Employees
                    (EmployeeCode, Name, Email, MobileNumber, Gender, DOB, ProfilePhotoPath, RoleId, Password, Status, CreatedBy, CreatedDateTime)
                OUTPUT INSERTED.Id
                VALUES
                    (@EmployeeCode, @Name, @Email, @MobileNumber, @Gender, @DOB, @ProfilePhotoPath, @RoleId, @Password, @Status, @CreatedBy, @CreatedDateTime)", con);
            AddParameters(cmd, employee);
            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // FIXED: Update employee without DepartmentId
        public async Task<int> UpdateAsync(Employee employee)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(@"
                UPDATE Employees SET
                    EmployeeCode = @EmployeeCode,
                    Name = @Name,
                    Email = @Email,
                    MobileNumber = @MobileNumber,
                    Gender = @Gender,
                    DOB = @DOB,
                    ProfilePhotoPath = @ProfilePhotoPath,
                    RoleId = @RoleId,
                    Password = @Password,
                    Status = @Status,
                    UpdatedBy = @UpdatedBy,
                    UpdatedDateTime = @UpdatedDateTime
                WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", employee.Id);
            AddParameters(cmd, employee, includeAudit: true);
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // FIXED: Get all employees without DepartmentId
        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var employees = new List<Employee>();
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(@"
                SELECT Id, EmployeeCode, Name, Email, MobileNumber, Gender, DOB, 
                       ProfilePhotoPath, RoleId, Password, Status, 
                       CreatedBy, CreatedDateTime, UpdatedBy, UpdatedDateTime 
                FROM Employees", con);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                employees.Add(MapEmployee(reader));
            }
            return employees;
        }

        // FIXED: Get employee by ID without DepartmentId
        public async Task<Employee?> GetByIdAsync(int id)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand(@"
                SELECT Id, EmployeeCode, Name, Email, MobileNumber, Gender, DOB, 
                       ProfilePhotoPath, RoleId, Password, Status, 
                       CreatedBy, CreatedDateTime, UpdatedBy, UpdatedDateTime 
                FROM Employees WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await con.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapEmployee(reader) : null;
        }

        // Delete employee
        public async Task<int> DeleteAsync(int id)
        {
            await using var con = new SqlConnection(_connectionString);
            await using var cmd = new SqlCommand("DELETE FROM Employees WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // ====== Helper Methods ======

        // FIXED: Employee mapping without DepartmentId
        private static Employee MapEmployee(SqlDataReader reader)
        {
            try
            {
                return new Employee
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    EmployeeCode = reader.GetString(reader.GetOrdinal("EmployeeCode")),
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    MobileNumber = reader.GetString(reader.GetOrdinal("MobileNumber")),
                    Gender = reader.GetString(reader.GetOrdinal("Gender")),
                    DOB = reader.IsDBNull(reader.GetOrdinal("DOB")) ? null : reader.GetDateTime(reader.GetOrdinal("DOB")),
                    ProfilePhotoPath = reader.IsDBNull(reader.GetOrdinal("ProfilePhotoPath")) ? null : reader.GetString(reader.GetOrdinal("ProfilePhotoPath")),
                    RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                    Password = reader.IsDBNull(reader.GetOrdinal("Password")) ? "DefaultHashedPassword" : reader.GetString(reader.GetOrdinal("Password")),
                    Status = reader.GetBoolean(reader.GetOrdinal("Status")),
                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                    CreatedDateTime = reader.GetDateTime(reader.GetOrdinal("CreatedDateTime")),
                    UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedBy")),
                    UpdatedDateTime = reader.IsDBNull(reader.GetOrdinal("UpdatedDateTime")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedDateTime"))
                };
            }
            catch (Exception ex)
            {
                throw new DataException($"Error mapping Employee: {ex.Message}", ex);
            }
        }

        // Normalize gender input
        private static string NormalizeGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("Gender cannot be empty. Allowed values: Male, Female, Other");
            gender = gender.Trim().ToLower();
            return gender switch
            {
                "male" or "m" => "Male",
                "female" or "f" => "Female",
                "other" or "o" => "Other",
                _ => throw new ArgumentException("Invalid gender value. Allowed values: Male, Female, Other")
            };
        }

        // Save profile photo file
        private async Task<string?> SaveProfilePhotoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;
            if (!Directory.Exists(_uploadsPath))
                Directory.CreateDirectory(_uploadsPath);
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(_uploadsPath, uniqueFileName);
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"/uploads/employees/{uniqueFileName}";
        }

        // FIXED: Add parameters without DepartmentId
        private static void AddParameters(SqlCommand cmd, Employee employee, bool includeAudit = false)
        {
            cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
            cmd.Parameters.AddWithValue("@Name", employee.Name);
            cmd.Parameters.AddWithValue("@Email", employee.Email);
            cmd.Parameters.AddWithValue("@MobileNumber", employee.MobileNumber);
            cmd.Parameters.AddWithValue("@Gender", NormalizeGender(employee.Gender));
            cmd.Parameters.AddWithValue("@DOB", (object?)employee.DOB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProfilePhotoPath", (object?)employee.ProfilePhotoPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RoleId", employee.RoleId);
            cmd.Parameters.AddWithValue("@Password", employee.Password);
            cmd.Parameters.AddWithValue("@Status", employee.Status);
            if (includeAudit)
            {
                cmd.Parameters.AddWithValue("@UpdatedBy", (object?)employee.UpdatedBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedDateTime", (object?)employee.UpdatedDateTime ?? DBNull.Value);
            }
            else
            {
                cmd.Parameters.AddWithValue("@CreatedBy", (object?)employee.CreatedBy ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedDateTime", employee.CreatedDateTime);
            }
        }
    }
}
