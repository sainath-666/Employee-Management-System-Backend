namespace Employee_Management_System_Backend.Model
{
    public class Department
    {
        public int Id { get; set; }  // Changed from DepartmentId to Id to match the database
        public string DepartmentName { get; set; } = string.Empty;  
        public bool Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
