using Employee_Management_System_Backend.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Data
{
    public class DepartmentRepository
    {
        private readonly string _conn;

        public DepartmentRepository(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Department>> GetAllAsync()
        {
            var list = new List<Department>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("SELECT * FROM Departments", conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Department
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                    Status = reader.GetBoolean(reader.GetOrdinal("Status")),
                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                    CreatedDateTime = reader.GetDateTime(reader.GetOrdinal("CreatedDateTime")),
                    UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedBy")),
                    UpdatedDateTime = reader.IsDBNull(reader.GetOrdinal("UpdatedDateTime")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedDateTime"))
                });
            }
            return list;
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            Department? dept = null;
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("SELECT * FROM Departments WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                dept = new Department
                {
                    Id = reader.GetInt32(reader.GetOrdinal("Id")),
                    DepartmentName = reader.GetString(reader.GetOrdinal("DepartmentName")),
                    Status = reader.GetBoolean(reader.GetOrdinal("Status")),
                    CreatedBy = reader.IsDBNull(reader.GetOrdinal("CreatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("CreatedBy")),
                    CreatedDateTime = reader.GetDateTime(reader.GetOrdinal("CreatedDateTime")),
                    UpdatedBy = reader.IsDBNull(reader.GetOrdinal("UpdatedBy")) ? null : reader.GetInt32(reader.GetOrdinal("UpdatedBy")),
                    UpdatedDateTime = reader.IsDBNull(reader.GetOrdinal("UpdatedDateTime")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedDateTime"))
                };
            }
            return dept;
        }

        public async Task AddAsync(Department dept)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand(
                "INSERT INTO Departments (DepartmentName, Status, CreatedBy, CreatedDateTime) " +
                "VALUES (@name, @status, @createdby, @createddatetime)", conn);

            cmd.Parameters.AddWithValue("@name", dept.DepartmentName);
            cmd.Parameters.AddWithValue("@status", dept.Status);
            cmd.Parameters.AddWithValue("@createdby", (object?)dept.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@createddatetime", dept.CreatedDateTime);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Department dept)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand(
                "UPDATE Departments " +
                "SET DepartmentName=@name, Status=@status, UpdatedBy=@updatedby, UpdatedDateTime=@updateddatetime " +
                "WHERE Id=@id", conn);

            cmd.Parameters.AddWithValue("@id", dept.Id);
            cmd.Parameters.AddWithValue("@name", dept.DepartmentName);
            cmd.Parameters.AddWithValue("@status", dept.Status);
            cmd.Parameters.AddWithValue("@updatedby", (object?)dept.UpdatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@updateddatetime", (object?)dept.UpdatedDateTime ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("DELETE FROM Departments WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
