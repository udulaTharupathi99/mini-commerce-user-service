using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MiniCommerce.UserService.Application.DTOs;
using MiniCommerce.UserService.Application.Interfaces;
using MiniCommerce.UserService.Domain.Entities;
using MiniCommerce.UserService.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace MiniCommerce.UserService.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userRepo;
        private readonly IRefreshTokenService _refreshRepo;
        private readonly IJwtProvider _jwtProvider;
        private readonly ILogger<AuthService> _logger;


        public AuthService(IUserService userRepo, IRefreshTokenService refreshRepo, IJwtProvider jwtProvider, ILogger<AuthService> logger)
        {
            _userRepo = userRepo;
            _refreshRepo = refreshRepo;
            _jwtProvider = jwtProvider;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepo.GetByEmailAsync(request.Email);
            if (existingUser != null)
                throw new InvalidOperationException("Email already exists");


            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);


            var user = new User
            {
                Email = request.Email,
                PasswordHash = passwordHash,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };


            await _userRepo.AddAsync(user);


            var accessToken = _jwtProvider.GenerateAccessToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();


            await _refreshRepo.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });


            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                FirstName = user.FirstName
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepo.GetByEmailAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Invalid login attempt for email {Email}", request.Email);
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            var accessToken = _jwtProvider.GenerateAccessToken(user);
            var refreshToken = _jwtProvider.GenerateRefreshToken();

            await _refreshRepo.AddAsync(new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });

            return new AuthResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Email = user.Email,
                FirstName = user.FirstName
            };
        }

        public async Task<AuthResponse> RefreshTokenAsync(string token)
        {
            var storedToken = await _refreshRepo.GetByTokenAsync(token);
            if (storedToken == null || storedToken.ExpiresAt < DateTime.UtcNow)
                throw new UnauthorizedAccessException("Invalid or expired refresh token");


            var user = await _userRepo.GetByIdAsync(storedToken.UserId);
            if (user == null) throw new UnauthorizedAccessException("User not found");


            var newAccessToken = _jwtProvider.GenerateAccessToken(user);
            var newRefreshToken = _jwtProvider.GenerateRefreshToken();


            // Remove old refresh token
            await _refreshRepo.RevokeAsync(storedToken);


            // Store new refresh token
            await _refreshRepo.AddAsync(new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });


            return new AuthResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Email = user.Email,
                FirstName = user.FirstName
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token is required");

            var storedToken = await _refreshRepo.GetByTokenAsync(refreshToken);
            if (storedToken != null)
            {
                // Revoke the refresh token
                await _refreshRepo.RevokeAsync(storedToken);
                _logger.LogInformation("User {UserId} logged out, refresh token revoked", storedToken.UserId);
                return true;
            }
            else
            {
                _logger.LogWarning("Attempted logout with invalid refresh token");
                return false;
            }
        }



    }
}
