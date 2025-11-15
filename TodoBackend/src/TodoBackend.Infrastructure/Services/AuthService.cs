using Microsoft.AspNetCore.Identity;
using TodoBackend.Core.DTOs;
using TodoBackend.Core.Entities;
using TodoBackend.Core.Interfaces;
using TodoBackend.Infrastructure.Entities;

namespace TodoBackend.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IJwtTokenService jwtTokenService)
        {
            _userManager = userManager;
            _jwtTokenService = jwtTokenService;
        }

        public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userManager.FindByNameAsync(request.UserName);
            if (existingUser != null)
            {
                return null; // Username already exists
            }

            var existingEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return null; // Email already exists
            }

            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                var token = _jwtTokenService.GenerateToken(user);
                return new AuthResponse
                {
                    Token = token,
                    Expiration = DateTime.UtcNow.AddMinutes(60), // Should match JWT expiration
                    UserName = user.UserName!,
                    Email = user.Email!
                };
            }

            return null;
        }

        public async Task<AuthResponse?> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user == null)
            {
                return null;
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return null;
            }

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            var token = _jwtTokenService.GenerateToken(user);
            return new AuthResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddMinutes(60), // Should match JWT expiration
                UserName = user.UserName!,
                Email = user.Email!
            };
        }
    }
}