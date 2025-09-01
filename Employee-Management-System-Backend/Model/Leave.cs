namespace Employee_Management_System_Backend.Model
{
    public class Leave
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public LeaveTypeEnum? LeaveTypeID { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MaxDaysPerYear { get; set; }
        public string Reason { get; set; }
        public StatusEnum Status { get; set; } = StatusEnum.Pending;
        public int? CreatedBy { get; set; }
        public DateTime CreatedDateTime { get; set; } = DateTime.Now;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDateTime { get; set; }
    }
}
