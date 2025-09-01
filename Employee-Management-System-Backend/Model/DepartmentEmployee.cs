namespace Employee_Management_System_Backend.Model
{
    public class DepartmentEmployee
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public int EmployeeId { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
