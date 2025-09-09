using System;
using System.ComponentModel.DataAnnotations;

namespace Employee_Management_System_Backend.Model
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee code is required")]
        [StringLength(20, ErrorMessage = "Employee code cannot exceed 20 characters")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(150)]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(15)]
        public required string MobileNumber { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        [RegularExpression("Male|Female|Other", ErrorMessage = "Gender must be Male, Female, or Other")]
        public required string Gender { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DOB { get; set; }

        [StringLength(250)]
        public string? ProfilePhotoPath { get; set; }

        [Required(ErrorMessage = "Role is required")]
        public required int RoleId { get; set; }

        // REMOVED: DepartmentId property since Departments table references EmployeeId
        // The relationship is: Departments.EmployeeId -> Employees.Id

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Password hash cannot exceed 255 characters")]
        public required string Password { get; set; }

        public bool Status { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
