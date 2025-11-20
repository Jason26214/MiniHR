namespace MiniHR.Domain.Exceptions
{
    public class DuplicateEmailException : Exception
    {
        public DuplicateEmailException(string email)
            : base($"Employee with email '{email}' already exists.")
        {
        }
    }
}
