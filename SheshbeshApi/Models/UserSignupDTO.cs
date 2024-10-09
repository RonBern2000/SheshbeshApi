﻿using System.ComponentModel.DataAnnotations;

namespace SheshbeshApi.Models
{
    public class UserSignupDTO
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Username must be between 2 and 30 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain English letters, numbers, and underscores.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(30, MinimumLength = 4, ErrorMessage = "Password must be between 4 and 30 characters.")]
        [RegularExpression(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{4,}$", ErrorMessage = "Password must contain at least one digit, one lowercase letter, and one uppercase letter.")]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [StringLength(70, ErrorMessage = "Email must not exceed 70 characters.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
    }
}
