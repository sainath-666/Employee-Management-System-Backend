using System;

namespace Employee_Management_System_Backend.Model
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = null!;
        public bool Status { get; set; } = true;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
