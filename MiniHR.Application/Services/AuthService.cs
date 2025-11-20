using MiniHR.Application.Interfaces;
using MiniHR.Application.POCOs.DTOs;
using MiniHR.Domain.Entities;
using MiniHR.Domain.Interfaces;

namespace MiniHR.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<User> RegisterAsync(RegisterDTO registerDto)
        {
            if (await _userRepository.ExistsByUsernameAsync(registerDto.Username))
            {
                throw new Exception("Username already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = registerDto.Username,
                PasswordHash = _passwordHasher.Hash(registerDto.Password),
                Role = registerDto.Role
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return user;
        }

        public async Task<string?> LoginAsync(LoginDTO loginDto)
        {
            var user = await _userRepository.GetByUsernameAsync(loginDto.Username);

            if (user == null)
            {
                return null;
            }

            if (!_passwordHasher.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            var token = _jwtTokenGenerator.GenerateToken(user);

            return token;
        }
    }
}