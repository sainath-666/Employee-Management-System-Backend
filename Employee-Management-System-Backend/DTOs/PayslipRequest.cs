using System.ComponentModel.DataAnnotations;

namespace Employee_Management_System_Backend.Model
{
    public class PayslipRequest
    {
        [Required(ErrorMessage = "Employee ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Employee ID must be a positive number")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CreatedBy must be a positive number")]
        public int CreatedBy { get; set; }
    }
}
