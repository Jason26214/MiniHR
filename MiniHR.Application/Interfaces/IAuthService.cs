using MiniHR.Application.POCOs.DTOs;
using MiniHR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniHR.Application.Interfaces
{
    public interface IAuthService
    {
        // Register: Receive the registration form, return the User entity upon successful registration
        // If registration fails (e.g., username already exists), an exception is usually thrown for the global exception handler to catch
        Task<User> RegisterAsync(RegisterDTO registerDto);

        // Login: Receives the login form and returns the generated Token string
        // If login fails (wrong password or user does not exist), returns null, and the Controller then converts it to a 401
        Task<string?> LoginAsync(LoginDTO loginDto);
    }
}
