namespace Employee_Management_System_Backend.Model
{
    public class EmployeeWithDepartment
    {
        public int Id { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string MobileNumber { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime? DOB { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public int RoleId { get; set; }
        public int? DepartmentId { get; set; }
        public string? DepartmentName { get; set; } // From Department table JOIN
        public decimal Salary { get; set; } // Added Salary property for payslip generation
        public bool Status { get; set; }
        public DateTime CreatedDateTime { get; set; }
    }
}
