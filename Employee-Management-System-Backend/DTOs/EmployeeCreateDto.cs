using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Employee_Management_System_Backend.DTOs
{
    public class EmployeeCreateDto
    {
        //[Required]
        //public required string EmployeeCode { get; set; }

        [Required]
        public required string Name { get; set; }

        [Required, EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string MobileNumber { get; set; }

        [Required]
        public required string Gender { get; set; }

        public DateTime? DOB { get; set; }

        public IFormFile? ProfilePhoto { get; set; } // optional file upload

        [Required]
        public int RoleId { get; set; }

        [Required]
        public required string Password { get; set; } // required for creation
    }
}
