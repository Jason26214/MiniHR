using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHR.Application.Interfaces
{
    public interface IPasswordHasher
    {
        // Hashes the given password and returns the hashed string
        string Hash(string password);
        // Verifies if the given password matches the hashed password
        bool Verify(string password, string hashedPassword);
    }
}
