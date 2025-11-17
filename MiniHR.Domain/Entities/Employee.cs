namespace MiniHR.Domain.Entities
{
    public class Employee
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        // Unique
        public string Email { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        // (18,2)
        public decimal Salary { get; set; }
        // Timezone
        public DateTimeOffset HireDate { get; set; }
        // Used for soft delete
        public bool IsDeleted { get; set; }
    }
}
