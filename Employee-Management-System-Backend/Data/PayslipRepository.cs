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

        // NEW: Update PDF path for existing payslip
        public async Task<int> UpdatePdfPathAsync(int payslipId, string pdfPath)
        {
            using var conn = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("UPDATE Payslips SET PdfPath = @PdfPath WHERE Id = @Id", conn);

            cmd.Parameters.AddWithValue("@PdfPath", pdfPath ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@Id", payslipId);

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // NEW: Add payslip and return the generated ID - FIXED: Removed NetSalary
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

            // NOTE: NetSalary parameter REMOVED - it's computed by database

            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
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

        // FIXED: Removed NetSalary from INSERT statement
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

            // NOTE: NetSalary parameter REMOVED - it's computed by database

            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        // FIXED: Removed NetSalary from UPDATE statement
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

            // NOTE: NetSalary parameter REMOVED - it's computed by database

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

        // Fixed MapToPayslip to use Convert and handle DBNull
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
                NetSalary = Convert.ToDecimal(reader["NetSalary"]), // Can still READ computed column
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
