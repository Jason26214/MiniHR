using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniHR.Application.Interfaces;
using MiniHR.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MiniHR.Infrastructure.Authentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        // to read configuration from appsettings.json
        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(User user)
        {
            var secretKey = _configuration["JwtSettings:Secret"];
            // Symmetric: This means that the same key is used for "encryption" and "decryption/verification."
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));

            // create signing credentials (part of the JwtSecurityToken)
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // claims
            var claims = new[]
            {
                // Sub (Subject): Typically holds the User ID
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                
                // UniqueName: Holds the Username
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                
                // Role: Holds the Role (This is crucial for the subsequent "AdminOnly" policy to take effect!)
                // Must use ClaimTypes.Role, otherwise ASP.NET's [Authorize(Roles=...)] won't recognise it
                new Claim(ClaimTypes.Role, user.Role),
                
                // Jti: Unique identifier for the Token, used to prevent replay attacks
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // create the token
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JwtSettings:ExpiryMinutes"]!)),
                signingCredentials: credentials
            );

            // Serialization (Write Token)
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
