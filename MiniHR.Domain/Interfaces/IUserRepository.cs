using MiniHR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHR.Domain.Interfaces
{
    public interface IUserRepository
    {
        // Find user by username (used for login)
        Task<User?> GetByUsernameAsync(string username);

        // Check if username exists (used for uniqueness check during registration)
        Task<bool> ExistsByUsernameAsync(string username);

        // Add a new user (used for registration)
        Task AddAsync(User user);

        // Save changes (Unit of Work)
        Task<int> SaveChangesAsync();
    }
}
