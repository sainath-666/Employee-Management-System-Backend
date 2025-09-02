using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Employee_Management_System_Backend.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.IO;

namespace Employee_Management_System_Backend.Data
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;
        private readonly string _uploadsPath;

        public EmployeeRepository(IConfiguration configuration, IOptions<EmployeeUploadSettings> uploadSettings)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new ArgumentNullException("Connection string 'DefaultConnection' is missing.");
            _uploadsPath = uploadSettings.Value.UploadsPath;
        }

        // ✅ Create Employee
        public async Task<int> CreateAsync(Employee employee)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO Employees
                (EmployeeCode, Name, Email, MobileNumber, Gender, DOB, ProfilePhotoPath, RoleId,
                 Password, Status, CreatedBy, CreatedDateTime)
                OUTPUT INSERTED.Id
                VALUES
                (@EmployeeCode, @Name, @Email, @MobileNumber, @Gender, @DOB, @ProfilePhotoPath, @RoleId,
                 @Password, @Status, @CreatedBy, @CreatedDateTime)", con);

            cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
            cmd.Parameters.AddWithValue("@Name", employee.Name);
            cmd.Parameters.AddWithValue("@Email", employee.Email);
            cmd.Parameters.AddWithValue("@MobileNumber", employee.MobileNumber);
            cmd.Parameters.AddWithValue("@Gender", NormalizeGender(employee.Gender));
            cmd.Parameters.AddWithValue("@DOB", (object?)employee.DOB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProfilePhotoPath", (object?)employee.ProfilePhotoPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RoleId", employee.RoleId);
            cmd.Parameters.AddWithValue("@Password", employee.Password); // NOT NULL
            cmd.Parameters.AddWithValue("@Status", employee.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)employee.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDateTime", employee.CreatedDateTime);

            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // ✅ Update Employee  
        public async Task<int> UpdateAsync(Employee employee)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE Employees SET
                    EmployeeCode = @EmployeeCode, Name = @Name, Email = @Email, MobileNumber = @MobileNumber,
                    Gender = @Gender, DOB = @DOB, ProfilePhotoPath = @ProfilePhotoPath, RoleId = @RoleId,
                    Password = @Password, Status = @Status,
                    UpdatedBy = @UpdatedBy, UpdatedDateTime = @UpdatedDateTime
                WHERE Id = @Id", con);

            cmd.Parameters.AddWithValue("@Id", employee.Id);
            cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
            cmd.Parameters.AddWithValue("@Name", employee.Name);
            cmd.Parameters.AddWithValue("@Email", employee.Email);
            cmd.Parameters.AddWithValue("@MobileNumber", employee.MobileNumber);
            cmd.Parameters.AddWithValue("@Gender", NormalizeGender(employee.Gender));
            cmd.Parameters.AddWithValue("@DOB", (object?)employee.DOB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProfilePhotoPath", (object?)employee.ProfilePhotoPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RoleId", employee.RoleId);
            cmd.Parameters.AddWithValue("@Password", employee.Password); // NOT NULL
            cmd.Parameters.AddWithValue("@Status", employee.Status);
            cmd.Parameters.AddWithValue("@UpdatedBy", (object?)employee.UpdatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedDateTime", (object?)employee.UpdatedDateTime ?? DBNull.Value);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // ✅ Get All Employees
        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            var employees = new List<Employee>();
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Employees", con);
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                employees.Add(MapEmployee(reader));
            }
            return employees;
        }

        // ✅ Get Employee By Id
        public async Task<Employee?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Employees WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapEmployee(reader) : null;
        }

        // ✅ Delete Employee
        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("DELETE FROM Employees WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // ✅ Updated Mapper with Password field (NOT NULL)
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
                    DOB = reader.IsDBNull(reader.GetOrdinal("DOB"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("DOB")),
                    ProfilePhotoPath = reader.IsDBNull(reader.GetOrdinal("ProfilePhotoPath"))
                        ? null
                        : reader.GetString(reader.GetOrdinal("ProfilePhotoPath")),
                    RoleId = reader.GetInt32(reader.GetOrdinal("RoleId")),
                    Password = reader.IsDBNull(reader.GetOrdinal("Password"))
                                ? "DefaultHashedPassword"
                                : reader.GetString(reader.GetOrdinal("Password")), // NOT NULL - required field
                    Status = reader.GetBoolean(reader.GetOrdinal("Status")),
                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                    CreatedDateTime = reader.GetDateTime(reader.GetOrdinal("CreatedDateTime")),
                    UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy"))
                        ? null
                        : reader.GetInt32(reader.GetOrdinal("UpdatedBy")),
                    UpdatedDateTime = reader.IsDBNull(reader.GetOrdinal("UpdatedDateTime"))
                        ? null
                        : reader.GetDateTime(reader.GetOrdinal("UpdatedDateTime"))
                };
            }
            catch (InvalidCastException ex)
            {
                throw new DataException($"Error converting database values to Employee object. Please check data types: {ex.Message}", ex);
            }
            catch (IndexOutOfRangeException ex)
            {
                throw new DataException($"Required column missing from query results: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new DataException($"Unexpected error mapping Employee data: {ex.Message}", ex);
            }
        }

        // ✅ Gender Normalizer
        private static string NormalizeGender(string? gender)
        {
            if (string.IsNullOrWhiteSpace(gender))
                throw new ArgumentException("Gender cannot be empty. Allowed values: Male, Female, Other");

            gender = gender.Trim().ToLower();
            return gender switch
            {
                "male" => "Male",
                "m" => "Male",
                "female" => "Female",
                "f" => "Female",
                "other" => "Other",
                "o" => "Other",
                _ => throw new ArgumentException("Invalid gender value. Allowed values: Male, Female, Other")
            };
        }

        private async Task<string?> SaveProfilePhotoAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            // Ensure directory exists
            if (!Directory.Exists(_uploadsPath))
                Directory.CreateDirectory(_uploadsPath);

            // Generate unique filename
            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var fullPath = Path.Combine(_uploadsPath, uniqueFileName);

            // Save file
            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Return URL-friendly path
            return $"/uploads/employees/{uniqueFileName}";
        }
    }
}
