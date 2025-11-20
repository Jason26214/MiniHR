using System;

namespace MiniHR.Application.POCOs.VOs
{
    public class EmployeeVO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public DateTimeOffset HireDate { get; set; }
    }
}
