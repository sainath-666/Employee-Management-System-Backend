using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient; // ✅ Use Microsoft.Data.SqlClient (better than System.Data.SqlClient)
using Employee_Management_System_Backend.Model;

namespace Employee_Management_System_Backend.Data
{
    public class EmployeeRepository
    {
        private readonly string _connectionString;

        // ✅ Constructor ensures _connectionString is never null
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

        // ✅ Mapper (uses SqlDataReader for performance)
        private static Employee MapEmployee(SqlDataReader reader)
        {
            return new Employee
            {
                Id = Convert.ToInt32(reader["Id"]),
                EmployeeCode = reader["EmployeeCode"].ToString()!,
                Name = reader["Name"].ToString()!,
                Email = reader["Email"].ToString()!,
                MobileNumber = reader["MobileNumber"].ToString()!,
                Gender = reader["Gender"].ToString()!,
                DOB = reader["DOB"] == DBNull.Value ? null : Convert.ToDateTime(reader["DOB"]),
                ProfilePhotoPath = reader["ProfilePhotoPath"] == DBNull.Value ? null : reader["ProfilePhotoPath"].ToString(),
                RoleId = Convert.ToInt32(reader["RoleId"]),
                PasswordHash = (byte[])reader["PasswordHash"],
                PasswordSalt = (byte[])reader["PasswordSalt"],
                Status = Convert.ToBoolean(reader["Status"]),
                CreatedBy = reader["CreatedBy"] == DBNull.Value ? null : Convert.ToInt32(reader["CreatedBy"]),
                CreatedDateTime = Convert.ToDateTime(reader["CreatedDateTime"]),
                UpdatedBy = reader["UpdatedBy"] == DBNull.Value ? null : Convert.ToInt32(reader["UpdatedBy"]),
                UpdatedDateTime = reader["UpdatedDateTime"] == DBNull.Value ? null : Convert.ToDateTime(reader["UpdatedDateTime"])
            };
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
