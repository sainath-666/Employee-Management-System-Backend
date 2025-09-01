using Employee_Management_System_Backend.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
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
            using var cmd = new SqlCommand("SELECT Id, DepartmentName FROM Departments", conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new Department
                {
                    Id = reader.GetInt32(0),
                    DepartmentName = reader.GetString(1)
                });
            }
            return list;
        }

        public async Task<Department?> GetByIdAsync(int id)
        {
            Department? dept = null;
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("SELECT Id, DepartmentName FROM Departments WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                dept = new Department
                {
                    Id = reader.GetInt32(0),
                    DepartmentName = reader.GetString(1)
                };
            }
            return dept;
        }

        public async Task AddAsync(Department dept)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("INSERT INTO Departments (DepartmentName) VALUES (@name)", conn);
            cmd.Parameters.AddWithValue("@name", dept.DepartmentName);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateAsync(Department dept)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand("UPDATE Departments SET DepartmentName=@name WHERE Id=@id", conn);
            cmd.Parameters.AddWithValue("@id", dept.Id);
            cmd.Parameters.AddWithValue("@name", dept.DepartmentName);
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
