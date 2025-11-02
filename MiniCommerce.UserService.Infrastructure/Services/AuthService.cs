using Microsoft.AspNetCore.Identity.Data;
using MiniCommerce.UserService.Application.DTOs;
using MiniCommerce.UserService.Application.Interfaces;
using MiniCommerce.UserService.Domain.Entities;
using MiniCommerce.UserService.Domain.Enums;
using MiniCommerce.UserService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoginRequest = MiniCommerce.UserService.Application.DTOs.LoginRequest;
using RegisterRequest = MiniCommerce.UserService.Application.DTOs.RegisterRequest;

namespace MiniCommerce.UserService.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _repo;
        private readonly JwtTokenGenerator _tokenGenerator;

        public AuthService(IUserRepository repo, JwtTokenGenerator tokenGenerator)
        {
            _repo = repo;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            var existing = await _repo.GetByEmailAsync(request.Email, ct);
            if (existing != null) throw new InvalidOperationException("User with this email already exists.");

            var hashed = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Email = request.Email,
                PasswordHash = hashed,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = UserRole.Customer
            };

            await _repo.AddAsync(user, ct);
            await _repo.SaveChangesAsync(ct);

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<(UserDto User, string Token)> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _repo.GetByEmailAsync(request.Email, ct);
            if (user == null) throw new UnauthorizedAccessException("Invalid credentials.");

            var ok = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!ok) throw new UnauthorizedAccessException("Invalid credentials.");

            var dto = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            var token = _tokenGenerator.GenerateToken(user);
            return (dto, token);
        }

        public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var user = await _repo.GetByIdAsync(id, ct);
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }


    }
}
