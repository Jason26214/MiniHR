using Microsoft.EntityFrameworkCore;
using MiniHR.Domain.Entities;
using MiniHR.Domain.Interfaces;
using MiniHR.Infrastructure.Persistence;

namespace MiniHR.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MiniHrDbContext _context;

        public UserRepository(MiniHrDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> ExistsByUsernameAsync(string username)
        {
            return await _context.Users
                .AnyAsync(u => u.Username == username);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
