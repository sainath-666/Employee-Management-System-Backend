namespace Employee_Management_System_Backend.Model
{
    public class PayslipRequest
    {
        public int EmployeeId { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Allowances { get; set; }
        public decimal Deductions { get; set; }
        public decimal NetSalary { get; set; }
        public string? Month { get; set; }
        public string? EmployeeName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? Email { get; set; }
    }
}
