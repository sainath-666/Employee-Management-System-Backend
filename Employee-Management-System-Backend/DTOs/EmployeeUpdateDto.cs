using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Employee_Management_System_Backend.DTOs
{
    public class EmployeeUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public required string EmployeeCode { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string MobileNumber { get; set; }

        [Required]
        public required string Gender { get; set; }

        public DateTime? DOB { get; set; }

        public IFormFile? ProfilePhoto { get; set; }

        [Required]
        public int RoleId { get; set; }

        public bool Status { get; set; } = true;

        public int? UpdatedBy { get; set; }

        public string? Password { get; set; } // optional
    }
}
