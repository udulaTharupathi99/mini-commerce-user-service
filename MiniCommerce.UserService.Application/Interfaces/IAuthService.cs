using MiniCommerce.UserService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCommerce.UserService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
        Task<(UserDto User, string Token)> LoginAsync(LoginRequest request, CancellationToken ct = default);
        Task<UserDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    }
}

