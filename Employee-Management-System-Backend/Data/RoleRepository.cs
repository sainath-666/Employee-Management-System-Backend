using Employee_Management_System_Backend.Model;
using Microsoft.Data.SqlClient;

namespace Employee_Management_System_Backend.Data
{
    public class RoleRepository
    {
        private readonly string _connectionString;

        public RoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(
                    nameof(configuration),
                    "Connection string 'DefaultConnection' is not found in appsettings.json"
                );
        }

        // Get all roles 
        public async Task<List<Role>> GetAllRolesAsync()
        {
            var roles = new List<Role>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM Roles";
                using SqlCommand cmd = new SqlCommand(sql, conn);
                await conn.OpenAsync();
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    roles.Add(MapRole(reader));
                }
            }

            return roles;
        }

        // Get role by id
        public async Task<Role?> GetRoleByIdAsync(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            string sql = "SELECT * FROM Roles WHERE Id=@Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapRole(reader);
            }
            return null;
        }

        // Add new role
        public async Task<int> AddRoleAsync(Role role)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            string sql = @"INSERT INTO Roles (RoleName, Status, CreatedBy, CreatedDateTime, UpdatedBy, UpdatedDateTime)
                          OUTPUT INSERTED.Id
                          VALUES (@RoleName, @Status, @CreatedBy, @CreatedDateTime, @UpdatedBy, @UpdatedDateTime)";
            
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
            cmd.Parameters.AddWithValue("@Status", role.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", role.CreatedBy as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDateTime", role.CreatedDateTime);
            cmd.Parameters.AddWithValue("@UpdatedBy", role.UpdatedBy as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedDateTime", role.UpdatedDateTime as object ?? DBNull.Value);

            await conn.OpenAsync();
            int newId = (int)await cmd.ExecuteScalarAsync();
            return newId;
        }

        // Update role
        public async Task<bool> UpdateRoleAsync(Role role)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            string sql = @"UPDATE Roles 
                          SET RoleName = @RoleName,
                              Status = @Status,
                              CreatedBy = @CreatedBy,
                              CreatedDateTime = @CreatedDateTime,
                              UpdatedBy = @UpdatedBy,
                              UpdatedDateTime = @UpdatedDateTime
                          WHERE Id = @Id";

            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", role.Id);
            cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
            cmd.Parameters.AddWithValue("@Status", role.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", role.CreatedBy as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDateTime", role.CreatedDateTime);
            cmd.Parameters.AddWithValue("@UpdatedBy", role.UpdatedBy as object ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UpdatedDateTime", role.UpdatedDateTime);

            await conn.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        // Delete role
        public async Task<bool> DeleteRoleAsync(int id)
        {
            using SqlConnection conn = new SqlConnection(_connectionString);
            string sql = "DELETE FROM Roles WHERE Id = @Id";
            using SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        private static Role MapRole(SqlDataReader reader)
        {
            return new Role
            {
                Id = Convert.ToInt32(reader["Id"]),
                RoleName = reader["RoleName"].ToString()!,
                Status = Convert.ToBoolean(reader["Status"]),
                CreatedBy = reader["CreatedBy"] != DBNull.Value ? Convert.ToInt32(reader["CreatedBy"]) : null,
                CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                UpdatedBy = reader["UpdatedBy"] != DBNull.Value ? Convert.ToInt32(reader["UpdatedBy"]) : null,
                UpdatedDateTime = reader["UpdatedDateTime"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedDateTime"]) : null
            };
        }
    }
}
