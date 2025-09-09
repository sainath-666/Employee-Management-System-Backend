using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Employee_Management_System_Backend.Model
{
    // Existing DTO for bulk CREATE operations (POST)
    public class BulkPayslipRequest
    {
        [Required(ErrorMessage = "At least one Employee ID is required")]
        [MinLength(1, ErrorMessage = "At least one Employee ID must be provided")]
        public List<int> EmployeeIds { get; set; } = new List<int>();

        [Required(ErrorMessage = "CreatedBy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CreatedBy must be a positive number")]
        public int CreatedBy { get; set; }
    }

    // UPDATED DTO for bulk UPDATE operations (PUT) - Using Employee IDs instead of Payslip IDs
    public class BulkPayslipUpdateRequest
    {
        [Required(ErrorMessage = "At least one Employee ID is required")]
        [MinLength(1, ErrorMessage = "At least one Employee ID must be provided")]
        public List<int> EmployeeIds { get; set; } = new List<int>(); // Changed from PayslipIds to EmployeeIds

        [Required(ErrorMessage = "CreatedBy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CreatedBy must be a positive number")]
        public int CreatedBy { get; set; }
    }

    // SIMPLIFIED DTO for single UPDATE operation (PUT) - No PayslipId needed
    public class PayslipUpdateRequest
    {
        [Required(ErrorMessage = "Employee ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Employee ID must be a positive number")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CreatedBy must be a positive number")]
        public int CreatedBy { get; set; }
    }
}
