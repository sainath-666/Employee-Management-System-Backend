using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Employee_Management_System_Backend.Model;

namespace Employee_Management_System_Backend.Data
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new ArgumentNullException("Connection string 'DefaultConnection' is missing.");
        }

        // ✅ Get All Employees
        public List<Employee> GetAll()
        {
            var employees = new List<Employee>();

            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Employees", con);

            con.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                employees.Add(MapEmployee(reader));
            }

            return employees;
        }

        // ✅ Get Employee By Id
        public Employee? GetById(int id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Employees WHERE Id = @Id", con);

            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();

            using var reader = cmd.ExecuteReader();
            return reader.Read() ? MapEmployee(reader) : null;
        }

        // ✅ Create Employee
        public void Create(Employee employee)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO Employees
                (EmployeeCode, Name, Email, MobileNumber, Gender, DOB, ProfilePhotoPath, RoleId,
                 PasswordHash, PasswordSalt, Status, CreatedBy, CreatedDateTime)
                VALUES
                (@EmployeeCode, @Name, @Email, @MobileNumber, @Gender, @DOB, @ProfilePhotoPath, @RoleId,
                 @PasswordHash, @PasswordSalt, @Status, @CreatedBy, @CreatedDateTime)", con);

            cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
            cmd.Parameters.AddWithValue("@Name", employee.Name);
            cmd.Parameters.AddWithValue("@Email", employee.Email);
            cmd.Parameters.AddWithValue("@MobileNumber", employee.MobileNumber);

            // ✅ Normalize Gender
            cmd.Parameters.AddWithValue("@Gender", NormalizeGender(employee.Gender));

            cmd.Parameters.AddWithValue("@DOB", (object?)employee.DOB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProfilePhotoPath", (object?)employee.ProfilePhotoPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RoleId", employee.RoleId);
            cmd.Parameters.AddWithValue("@PasswordHash", employee.PasswordHash);
            cmd.Parameters.AddWithValue("@PasswordSalt", employee.PasswordSalt);
            cmd.Parameters.AddWithValue("@Status", employee.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)employee.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDateTime", employee.CreatedDateTime);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // ✅ Update Employee
        public void Update(Employee employee)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE Employees SET
                    EmployeeCode = @EmployeeCode, Name = @Name, Email = @Email, MobileNumber = @MobileNumber,
                    Gender = @Gender, DOB = @DOB, ProfilePhotoPath = @ProfilePhotoPath, RoleId = @RoleId,
                    PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt, Status = @Status,
                    UpdatedBy = @UpdatedBy, UpdatedDateTime = @UpdatedDateTime
                WHERE Id = @Id", con);

            cmd.Parameters.AddWithValue("@Id", employee.Id);
            cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
            cmd.Parameters.AddWithValue("@Name", employee.Name);
            cmd.Parameters.AddWithValue("@Email", employee.Email);
            cmd.Parameters.AddWithValue("@MobileNumber", employee.MobileNumber);

            // ✅ Normalize Gender
            cmd.Parameters.AddWithValue("@Gender", NormalizeGender(employee.Gender));

            cmd.Parameters.AddWithValue("@DOB", (object?)employee.DOB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProfilePhotoPath", (object?)employee.ProfilePhotoPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RoleId", employee.RoleId);
            cmd.Parameters.AddWithValue("@PasswordHash", employee.PasswordHash);
            cmd.Parameters.AddWithValue("@PasswordSalt", employee.PasswordSalt);
            cmd.Parameters.AddWithValue("@Status", employee.Status);
            cmd.Parameters.AddWithValue("@UpdatedBy", (object?)employee.UpdatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedDateTime", (object?)employee.UpdatedDateTime ?? DBNull.Value);

            con.Open();
            cmd.ExecuteNonQuery();
        }

        // ✅ Delete Employee
        public void Delete(int id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("DELETE FROM Employees WHERE Id = @Id", con);

            cmd.Parameters.AddWithValue("@Id", id);
            con.Open();
            cmd.ExecuteNonQuery();
        }

        // Async interface implementations
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

        public async Task<Employee?> GetByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Employees WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapEmployee(reader) : null;
        }

        public async Task<int> CreateAsync(Employee employee)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO Employees
                (EmployeeCode, Name, Email, MobileNumber, Gender, DOB, ProfilePhotoPath, RoleId,
                 PasswordHash, PasswordSalt, Status, CreatedBy, CreatedDateTime)
                OUTPUT INSERTED.Id
                VALUES
                (@EmployeeCode, @Name, @Email, @MobileNumber, @Gender, @DOB, @ProfilePhotoPath, @RoleId,
                 @PasswordHash, @PasswordSalt, @Status, @CreatedBy, @CreatedDateTime)", con);

            cmd.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
            cmd.Parameters.AddWithValue("@Name", employee.Name);
            cmd.Parameters.AddWithValue("@Email", employee.Email);
            cmd.Parameters.AddWithValue("@MobileNumber", employee.MobileNumber);
            cmd.Parameters.AddWithValue("@Gender", NormalizeGender(employee.Gender));
            cmd.Parameters.AddWithValue("@DOB", (object?)employee.DOB ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ProfilePhotoPath", (object?)employee.ProfilePhotoPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RoleId", employee.RoleId);
            cmd.Parameters.AddWithValue("@PasswordHash", employee.PasswordHash);
            cmd.Parameters.AddWithValue("@PasswordSalt", employee.PasswordSalt);
            cmd.Parameters.AddWithValue("@Status", employee.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)employee.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDateTime", employee.CreatedDateTime);

            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<int> UpdateAsync(Employee employee)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE Employees SET
                    EmployeeCode = @EmployeeCode, Name = @Name, Email = @Email, MobileNumber = @MobileNumber,
                    Gender = @Gender, DOB = @DOB, ProfilePhotoPath = @ProfilePhotoPath, RoleId = @RoleId,
                    PasswordHash = @PasswordHash, PasswordSalt = @PasswordSalt, Status = @Status,
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
            cmd.Parameters.AddWithValue("@PasswordHash", employee.PasswordHash);
            cmd.Parameters.AddWithValue("@PasswordSalt", employee.PasswordSalt);
            cmd.Parameters.AddWithValue("@Status", employee.Status);
            cmd.Parameters.AddWithValue("@UpdatedBy", (object?)employee.UpdatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedDateTime", (object?)employee.UpdatedDateTime ?? DBNull.Value);

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("DELETE FROM Employees WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // ✅ Mapper (uses SqlDataReader for performance)
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
                    //PasswordHash = (byte[])reader["PasswordHash"],
                    //PasswordSalt = (byte[])reader["PasswordSalt"],
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
    }
}
