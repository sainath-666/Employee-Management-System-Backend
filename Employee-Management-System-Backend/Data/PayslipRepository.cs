using Employee_Management_System_Backend.Model;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Data
{
    public class PayslipRepository : IPayslipRepository
    {
        private readonly string _connectionString;

        public PayslipRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException(nameof(config), "Connection string 'DefaultConnection' is missing.");
        }

        // Interface method implementations
        public Task<IEnumerable<Payslip>> GetPayslipsAsync() => GetAllAsync();
        public Task<Payslip?> GetPayslipByIdAsync(int id) => GetByIdAsync(id);
        public Task<int> AddPayslipAsync(Payslip ps) => CreateAsync(ps);
        public Task<int> UpdatePayslipAsync(Payslip ps) => UpdateAsync(ps);
        public Task<int> DeletePayslipAsync(int id) => DeleteAsync(id);

        // Update PDF path for existing payslip
        public async Task<int> UpdatePdfPathAsync(int payslipId, string pdfPath)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("UPDATE Payslips SET PdfPath = @PdfPath WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@PdfPath", pdfPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", payslipId);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // Add payslip and return the generated ID
        public async Task<int> AddPayslipWithReturnIdAsync(Payslip payslip)
        {
            var query = @"INSERT INTO Payslips 
                (EmployeeId, Salary, BaseSalary, Allowances, Deductions, PdfPath, Month, Status, CreatedBy, CreatedDateTime) 
                OUTPUT INSERTED.Id
                VALUES (@EmployeeId, @Salary, @BaseSalary, @Allowances, @Deductions, @PdfPath, @Month, @Status, @CreatedBy, @CreatedDateTime)";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@EmployeeId", payslip.EmployeeId);
            cmd.Parameters.AddWithValue("@Salary", payslip.Salary);
            cmd.Parameters.AddWithValue("@BaseSalary", payslip.BaseSalary);
            cmd.Parameters.AddWithValue("@Allowances", payslip.Allowances);
            cmd.Parameters.AddWithValue("@Deductions", payslip.Deductions);
            cmd.Parameters.AddWithValue("@PdfPath", (object?)payslip.PdfPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Month", (object?)payslip.Month ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", payslip.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)payslip.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDateTime", payslip.CreatedDateTime);

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // CORRECTED: Get payslip with employee details including department name
        public async Task<PayslipWithEmployee?> GetPayslipWithEmployeeAsync(int payslipId)
        {
            var query = @"
                SELECT 
                    p.Id, p.EmployeeId, e.Name as EmployeeName, e.EmployeeCode,
                    p.Salary, p.BaseSalary, p.Allowances, p.Deductions, p.NetSalary, 
                    p.Month, p.Status, p.PdfPath, p.CreatedDateTime,
                    d.DepartmentName as DepartmentName
                FROM Payslips p
                INNER JOIN Employees e ON p.EmployeeId = e.Id
                LEFT JOIN DepartmentEmployees de ON e.Id = de.EmployeeId
                LEFT JOIN Departments d ON de.DepartmentId = d.Id
                WHERE p.Id = @PayslipId";

            try
            {
                await using var con = new SqlConnection(_connectionString);
                await using var cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@PayslipId", payslipId);

                await con.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    return new PayslipWithEmployee
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                        EmployeeName = reader.GetString(reader.GetOrdinal("EmployeeName")),
                        EmployeeCode = reader.GetString(reader.GetOrdinal("EmployeeCode")),
                        Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                        BaseSalary = reader.GetDecimal(reader.GetOrdinal("BaseSalary")),
                        Allowances = reader.GetDecimal(reader.GetOrdinal("Allowances")),
                        Deductions = reader.GetDecimal(reader.GetOrdinal("Deductions")),
                        NetSalary = reader.GetDecimal(reader.GetOrdinal("NetSalary")),
                        Month = reader.IsDBNull(reader.GetOrdinal("Month")) ? null : reader.GetString(reader.GetOrdinal("Month")),
                        Status = reader.GetBoolean(reader.GetOrdinal("Status")),
                        PdfPath = reader.IsDBNull(reader.GetOrdinal("PdfPath")) ? null : reader.GetString(reader.GetOrdinal("PdfPath")),
                        DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) ? null : reader.GetString(reader.GetOrdinal("DepartmentName")),
                        CreatedDateTime = reader.GetDateTime(reader.GetOrdinal("CreatedDateTime"))
                    };
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payslip with employee data: {ex.Message}", ex);
            }
        }

        // Private actual implementations
        private async Task<IEnumerable<Payslip>> GetAllAsync()
        {
            var payslips = new List<Payslip>();
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Payslips ORDER BY CreatedDateTime DESC", conn);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                payslips.Add(MapToPayslip(reader));
            }
            return payslips;
        }

        private async Task<Payslip?> GetByIdAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Payslips WHERE Id = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            return await reader.ReadAsync() ? MapToPayslip(reader) : null;
        }

        private async Task<int> CreateAsync(Payslip ps)
        {
            var query = @"INSERT INTO Payslips 
                (EmployeeId, Salary, BaseSalary, Allowances, Deductions, PdfPath, Month, Status, CreatedBy, CreatedDateTime) 
                VALUES (@EmployeeId, @Salary, @BaseSalary, @Allowances, @Deductions, @PdfPath, @Month, @Status, @CreatedBy, @CreatedDateTime)";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@EmployeeId", ps.EmployeeId);
            cmd.Parameters.AddWithValue("@Salary", ps.Salary);
            cmd.Parameters.AddWithValue("@BaseSalary", ps.BaseSalary);
            cmd.Parameters.AddWithValue("@Allowances", ps.Allowances);
            cmd.Parameters.AddWithValue("@Deductions", ps.Deductions);
            cmd.Parameters.AddWithValue("@PdfPath", (object?)ps.PdfPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Month", (object?)ps.Month ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", ps.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", (object?)ps.CreatedBy ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CreatedDateTime", ps.CreatedDateTime);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<int> UpdateAsync(Payslip ps)
        {
            var query = @"UPDATE Payslips SET 
                Salary=@Salary, BaseSalary=@BaseSalary, Allowances=@Allowances, 
                Deductions=@Deductions, PdfPath=@PdfPath, Month=@Month, Status=@Status,
                UpdatedBy=@UpdatedBy, UpdatedDateTime=GETDATE()
                WHERE Id=@Id";

            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Id", ps.Id);
            cmd.Parameters.AddWithValue("@Salary", ps.Salary);
            cmd.Parameters.AddWithValue("@BaseSalary", ps.BaseSalary);
            cmd.Parameters.AddWithValue("@Allowances", ps.Allowances);
            cmd.Parameters.AddWithValue("@Deductions", ps.Deductions);
            cmd.Parameters.AddWithValue("@PdfPath", (object?)ps.PdfPath ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Month", (object?)ps.Month ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", ps.Status);
            cmd.Parameters.AddWithValue("@UpdatedBy", (object?)ps.UpdatedBy ?? DBNull.Value);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        private async Task<int> DeleteAsync(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("DELETE FROM Payslips WHERE Id=@Id", conn);
            cmd.Parameters.AddWithValue("@Id", id);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        private static Payslip MapToPayslip(SqlDataReader reader)
        {
            return new Payslip
            {
                Id = Convert.ToInt32(reader["Id"]),
                EmployeeId = Convert.ToInt32(reader["EmployeeId"]),
                Salary = Convert.ToDecimal(reader["Salary"]),
                BaseSalary = Convert.ToDecimal(reader["BaseSalary"]),
                Allowances = Convert.ToDecimal(reader["Allowances"]),
                Deductions = Convert.ToDecimal(reader["Deductions"]),
                NetSalary = Convert.ToDecimal(reader["NetSalary"]),
                PdfPath = reader["PdfPath"] == DBNull.Value ? null : reader["PdfPath"].ToString(),
                Month = reader["Month"] == DBNull.Value ? null : reader["Month"].ToString(),
                Status = Convert.ToBoolean(reader["Status"]),
                CreatedBy = reader["CreatedBy"] == DBNull.Value ? null : (int?)reader["CreatedBy"],
                CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                UpdatedBy = reader["UpdatedBy"] == DBNull.Value ? null : (int?)reader["UpdatedBy"],
                UpdatedDateTime = reader["UpdatedDateTime"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDateTime"]
            };
        }
    }
}
