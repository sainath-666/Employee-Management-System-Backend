
using Employee_Management_System_Backend.Model;
using System.Data;
using System.Data.SqlClient;

namespace Employee_Management_System_Backend.Data
{
    public class PayslipRepository
    {
        private readonly string _connectionString;

        public PayslipRepository(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // Get all payslips
        public async Task<List<Payslip>> GetPayslipsAsync()
        {
            var payslips = new List<Payslip>();

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT * FROM Payslips", conn))
            {
                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        payslips.Add(MapToPayslip(reader));
                    }
                }
            }

            return payslips;
        }

        // Get by ID
        public async Task<Payslip?> GetPayslipByIdAsync(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("SELECT * FROM Payslips WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                await conn.OpenAsync();
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return MapToPayslip(reader);
                    }
                }
            }
            return null;
        }

        // Insert
        public async Task<int> AddPayslipAsync(Payslip ps)
        {
            var query = @"INSERT INTO Payslips 
                (EmployeeId, Salary, BaseSalary, Allowances, Deductions, PdfPath, Month, CreatedBy) 
                VALUES (@EmployeeId, @Salary, @BaseSalary, @Allowances, @Deductions, @PdfPath, @Month, @CreatedBy)";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@EmployeeId", ps.EmployeeId);
                cmd.Parameters.AddWithValue("@Salary", ps.Salary);
                cmd.Parameters.AddWithValue("@BaseSalary", ps.BaseSalary);
                cmd.Parameters.AddWithValue("@Allowances", ps.Allowances);
                cmd.Parameters.AddWithValue("@Deductions", ps.Deductions);
                cmd.Parameters.AddWithValue("@PdfPath", (object?)ps.PdfPath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)ps.Month ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CreatedBy", (object?)ps.CreatedBy ?? DBNull.Value);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        // Update
        public async Task<int> UpdatePayslipAsync(Payslip ps)
        {
            var query = @"UPDATE Payslips SET 
                Salary = @Salary, BaseSalary = @BaseSalary, Allowances = @Allowances, 
                Deductions = @Deductions, PdfPath = @PdfPath, Month = @Month, 
                UpdatedBy = @UpdatedBy, UpdatedDateTime = GETDATE()
                WHERE Id = @Id";

            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@Id", ps.Id);
                cmd.Parameters.AddWithValue("@Salary", ps.Salary);
                cmd.Parameters.AddWithValue("@BaseSalary", ps.BaseSalary);
                cmd.Parameters.AddWithValue("@Allowances", ps.Allowances);
                cmd.Parameters.AddWithValue("@Deductions", ps.Deductions);
                cmd.Parameters.AddWithValue("@PdfPath", (object?)ps.PdfPath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)ps.Month ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UpdatedBy", (object?)ps.UpdatedBy ?? DBNull.Value);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        // Delete
        public async Task<int> DeletePayslipAsync(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand("DELETE FROM Payslips WHERE Id = @Id", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                await conn.OpenAsync();
                return await cmd.ExecuteNonQueryAsync();
            }
        }

        // Mapper (from SqlDataReader instead of DataRow)
        private Payslip MapToPayslip(SqlDataReader reader)
        {
            return new Payslip
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                EmployeeId = reader.GetInt32(reader.GetOrdinal("EmployeeId")),
                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                BaseSalary = reader.GetDecimal(reader.GetOrdinal("BaseSalary")),
                Allowances = reader.GetDecimal(reader.GetOrdinal("Allowances")),
                Deductions = reader.GetDecimal(reader.GetOrdinal("Deductions")),
                NetSalary = reader.GetDecimal(reader.GetOrdinal("NetSalary")),
                PdfPath = reader["PdfPath"] == DBNull.Value ? null : reader["PdfPath"].ToString(),
                Month = reader["Month"] == DBNull.Value ? null : reader["Month"].ToString(),
                Status = reader.GetBoolean(reader.GetOrdinal("Status")),
                CreatedBy = reader["CreatedBy"] == DBNull.Value ? null : (int?)reader["CreatedBy"],
                CreatedDateTime = reader.GetDateTime(reader.GetOrdinal("CreatedDateTime")),
                UpdatedBy = reader["UpdatedBy"] == DBNull.Value ? null : (int?)reader["UpdatedBy"],
                UpdatedDateTime = reader["UpdatedDateTime"] == DBNull.Value ? null : (DateTime?)reader["UpdatedDateTime"]
            };
        }
    }
}

