using ECommerce.Data;
using ECommerce.DTOs;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerce.Services
{
    // Services/AuthService.cs
    public interface IAuthService
    {
        Task<string> Authenticate(string username, string password);
        Task<User> Register(UserDto userDto);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> Authenticate(string username, string password)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null || !VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning("Authentication failed for user: {Username}", username);
                    return null;
                }

                var token = GenerateJwtToken(user);
                _logger.LogInformation("User {Username} authenticated successfully", username);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user: {Username}", username);
                throw;
            }
        }

        public async Task<User> Register(UserDto userDto)
        {
            try
            {
                if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
                {
                    throw new Exception("Username already exists");
                }

                var user = new User
                {
                    Username = userDto.Username,
                    PasswordHash = HashPassword(userDto.Password),
                    Role = "Customer"
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered: {Username}", userDto.Username);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user: {Username}", userDto.Username);
                throw;
            }
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(120),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
