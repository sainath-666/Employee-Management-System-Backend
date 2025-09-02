﻿using System.ComponentModel.DataAnnotations;

namespace Employee_Management_System_Backend.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
