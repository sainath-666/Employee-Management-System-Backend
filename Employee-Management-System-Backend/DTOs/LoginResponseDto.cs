namespace Employee_Management_System_Backend.DTOs
{
    public class LoginResponseDto
    {
        public required string Token { get; set; }
        public required string Message { get; set; }
        public int EmployeeId { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
    }
}
