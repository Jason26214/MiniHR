using System;

namespace MiniHR.Application.POJOs.DTOs
{
    public class EmployeeDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal Salary { get; set; }
        public DateTimeOffset HireDate { get; set; }
    }
}
