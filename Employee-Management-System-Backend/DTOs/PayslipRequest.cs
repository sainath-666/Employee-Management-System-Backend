using System.ComponentModel.DataAnnotations;
namespace Employee_Management_System_Backend.Model
{
    public class PayslipRequest
    {
        [Required(ErrorMessage = "Employee ID is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Employee ID must be a positive number")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Base salary is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Base salary must be greater than 0")]
        public decimal BaseSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Allowances cannot be negative")]
        public decimal Allowances { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Deductions cannot be negative")]
        public decimal Deductions { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        [Range(1, int.MaxValue, ErrorMessage = "CreatedBy must be a positive number")]
        public int CreatedBy { get; set; }
    }
}
