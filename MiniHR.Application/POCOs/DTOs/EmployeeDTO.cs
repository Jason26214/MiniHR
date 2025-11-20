using System;
using System.ComponentModel.DataAnnotations;

namespace MiniHR.Application.POCOs.DTOs
{
    public class EmployeeDTO
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Position { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTimeOffset HireDate { get; set; }
    }
}
