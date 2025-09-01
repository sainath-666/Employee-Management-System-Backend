using Employee_Management_System_Backend.Model;
using System;
using System.Collections.Generic;
using System.Data;
//using System.Data.SqlClient;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace Employee_Management_System_Backend.Data
{
    public class LeaveRepository
    {
        private readonly string _connectionString;

        public LeaveRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Leave>> GetAllLeave()
        {
            var leaves = new List<Leave>();
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Leaves", con);
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                leaves.Add(new Leave
                {
                    Id = (int)reader["Id"],
                    EmployeeId = (int)reader["EmployeeId"],
                    LeaveTypeID = reader["LeaveTypeID"] != DBNull.Value ? (LeaveTypeEnum?)Convert.ToInt32(reader["LeaveTypeID"]) : null,
                    StartDate = reader["StartDate"] as DateTime?,
                    EndDate = reader["EndDate"] as DateTime?,
                    MaxDaysPerYear = reader["MaxDaysPerYear"] as int?,
                    Reason = reader["Reason"] as string,
                    Status = (StatusEnum)(reader["Status"]),
                    CreatedBy = reader["CreatedBy"] as int?,
                    CreatedDateTime = (DateTime)reader["CreatedDateTime"],
                    UpdatedBy = reader["UpdatedBy"] as int?,
                    UpdatedDateTime = reader["UpdatedDateTime"] as DateTime?
                });
            }
            return leaves;
        }

        public async Task<Leave?> GetLeaveById(int id)
        {
            Leave? leave = null;
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("SELECT * FROM Leaves WHERE Id=@Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                leave = new Leave
                {
                    Id = (int)reader["Id"],
                    EmployeeId = (int)reader["EmployeeId"],
                    LeaveTypeID = reader["LeaveTypeID"] != DBNull.Value ? (LeaveTypeEnum?)Convert.ToInt32(reader["LeaveTypeID"]) : null,
                    StartDate = reader["StartDate"] as DateTime?,
                    EndDate = reader["EndDate"] as DateTime?,
                    MaxDaysPerYear = reader["MaxDaysPerYear"] as int?,
                    Reason = reader["Reason"] as string,
                    Status = (StatusEnum)(reader["Status"]),
                    CreatedBy = reader["CreatedBy"] as int?,
                    CreatedDateTime = (DateTime)reader["CreatedDateTime"],
                    UpdatedBy = reader["UpdatedBy"] as int?,
                    UpdatedDateTime = reader["UpdatedDateTime"] as DateTime?
                };
            }
            return leave;
        }

        public async Task CreateLeave(Leave leave)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                INSERT INTO Leaves (EmployeeId, LeaveTypeID, StartDate, EndDate, MaxDaysPerYear, Reason, Status, CreatedBy)
                VALUES (@EmployeeId, @LeaveTypeID, @StartDate, @EndDate, @MaxDaysPerYear, @Reason, @Status, @CreatedBy)", con);

            cmd.Parameters.AddWithValue("@EmployeeId", leave.EmployeeId);
            cmd.Parameters.AddWithValue("@LeaveTypeID", leave.LeaveTypeID.HasValue ? (int)leave.LeaveTypeID.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDate", leave.StartDate.HasValue ? leave.StartDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", leave.EndDate.HasValue ? leave.EndDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxDaysPerYear", leave.MaxDaysPerYear.HasValue ? leave.MaxDaysPerYear.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Reason", leave.Reason ?? "");
            cmd.Parameters.AddWithValue("@Status", (int)leave.Status);
            cmd.Parameters.AddWithValue("@CreatedBy", leave.CreatedBy.HasValue ? leave.CreatedBy.Value : DBNull.Value);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateLeave(Leave leave)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand(@"
                UPDATE Leaves
                SET EmployeeId=@EmployeeId,
                    LeaveTypeID=@LeaveTypeID,
                    StartDate=@StartDate,
                    EndDate=@EndDate,
                    MaxDaysPerYear=@MaxDaysPerYear,
                    Reason=@Reason,
                    Status=@Status,
                    UpdatedBy=@UpdatedBy,
                    UpdatedDateTime=GETDATE()
                WHERE Id=@Id", con);

            cmd.Parameters.AddWithValue("@Id", leave.Id);
            cmd.Parameters.AddWithValue("@EmployeeId", leave.EmployeeId);
            cmd.Parameters.AddWithValue("@LeaveTypeID", leave.LeaveTypeID.HasValue ? (int)leave.LeaveTypeID.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@StartDate", leave.StartDate.HasValue ? leave.StartDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@EndDate", leave.EndDate.HasValue ? leave.EndDate.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@MaxDaysPerYear", leave.MaxDaysPerYear.HasValue ? leave.MaxDaysPerYear.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@Reason", leave.Reason ?? "");
            cmd.Parameters.AddWithValue("@Status", (int)leave.Status);
            cmd.Parameters.AddWithValue("@UpdatedBy", leave.UpdatedBy.HasValue ? leave.UpdatedBy.Value : DBNull.Value);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteLeave(int id)
        {
            using var con = new SqlConnection(_connectionString);
            using var cmd = new SqlCommand("DELETE FROM Leaves WHERE Id=@Id", con);
            cmd.Parameters.AddWithValue("@Id", id);
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
