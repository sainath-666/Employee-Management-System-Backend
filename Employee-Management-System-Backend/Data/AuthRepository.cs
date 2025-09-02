using Employee_Management_System_Backend.Model;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Employee_Management_System_Backend.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("Connection string missing.");
        }

        public async Task<Employee?> GetEmployeeByEmailAsync(string email)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Employees WHERE Email = @Email AND Status = 1", con);
            cmd.Parameters.AddWithValue("@Email", email);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapEmployee(reader) : null;
        }

        public async Task<bool> UpdateLastLoginAsync(int employeeId, DateTime loginTime)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(
                "UPDATE Employees SET UpdatedDateTime = @LoginTime WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", employeeId);
            cmd.Parameters.AddWithValue("@LoginTime", loginTime);

            await con.OpenAsync();
            var result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        }

        public async Task<Employee?> GetEmployeeByIdAsync(int id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Employees WHERE Id = @Id", con);
            cmd.Parameters.AddWithValue("@Id", id);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            return await reader.ReadAsync() ? MapEmployee(reader) : null;
        }

        // Updated MapEmployee method with proper NULL handling and GetOrdinal usage
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

                    // FIXED: Handle NULL Password values properly
                    Password = reader.IsDBNull(reader.GetOrdinal("Password"))
                        ? "DefaultHashedPassword"
                        : reader.GetString(reader.GetOrdinal("Password")),

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
            catch (Exception ex)
            {
                throw new DataException($"Error mapping Employee data in AuthRepository: {ex.Message}", ex);
            }
        }
    }
}
