using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MiniHR.Application.Interfaces;
using MiniHR.Application.POCOs.DTOs;
using MiniHR.Application.POCOs.VOs;
using MiniHR.Domain.Entities;

namespace MiniHR.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;

        public AuthController(IAuthService authService, IMapper mapper)
        {
            _authService = authService;
            _mapper = mapper;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<UserVO>> Register([FromBody] RegisterDTO registerDto)
        {
            User user = await _authService.RegisterAsync(registerDto);

            UserVO userVO = _mapper.Map<UserVO>(user);

            return Ok(userVO);
        }

        // POST: api/auth/login
        [HttpPost("login")]
        // Anonymous objects have no return type, and C# does not allow anonymous types in generics
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDto)
        {
            string? token = await _authService.LoginAsync(loginDto);

            if (token == null)
            {
                return Unauthorized("Invalid username or password.");
            }

            // return anonymous object with token property
            return Ok(new { token = token });
        }
    }
}