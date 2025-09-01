using Employee_Management_System_Backend.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Data
{
    public class DepartmentEmployeeRepository
    {
        private readonly string _conn;

        public DepartmentEmployeeRepository(IConfiguration config)
        {
            _conn = config.GetConnectionString("DefaultConnection");
        }

        public async Task AssignEmployeeToDepartmentsAsync(int employeeId, List<int> departmentIds)
        {
            using var conn = new SqlConnection(_conn);
            await conn.OpenAsync();

            foreach (var deptId in departmentIds)
            {
                using var cmd = new SqlCommand(
                    "INSERT INTO DepartmentEmployees (EmployeeId, DepartmentId) VALUES (@empId, @deptId)", conn);
                cmd.Parameters.AddWithValue("@empId", employeeId);
                cmd.Parameters.AddWithValue("@deptId", deptId);
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task<List<Department>> GetDepartmentsByEmployeeAsync(int employeeId)
        {
            var list = new List<Department>();
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand(@"
                SELECT d.Id, d.DepartmentName
                FROM DepartmentEmployees de
                JOIN Departments d ON de.DepartmentId = d.Id
                WHERE de.EmployeeId = @empId", conn);

            cmd.Parameters.AddWithValue("@empId", employeeId);
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

        public async Task RemoveDepartmentFromEmployeeAsync(int employeeId, int departmentId)
        {
            using var conn = new SqlConnection(_conn);
            using var cmd = new SqlCommand(
                "DELETE FROM DepartmentEmployees WHERE EmployeeId=@empId AND DepartmentId=@deptId", conn);
            cmd.Parameters.AddWithValue("@empId", employeeId);
            cmd.Parameters.AddWithValue("@deptId", departmentId);
            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
