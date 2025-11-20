using MiniHR.Application.Interfaces;

namespace MiniHR.Infrastructure.Authentication
{
    public class PasswordHasher : IPasswordHasher
    {
        // Automatically generate a salt and mix it with the password, resulting in a different encryption each time, which is very secure
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // Extract the Salt used at the time from the hashedPassword, then calculate it using the same algorithm to see if it matches
        public bool Verify(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
