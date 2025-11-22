using Microsoft.EntityFrameworkCore;
using MiniCommerce.UserService.Application.Interfaces;
using MiniCommerce.UserService.Domain.Entities;
using MiniCommerce.UserService.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniCommerce.UserService.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenService
    {
        private readonly UserDbContext _db;
        public RefreshTokenRepository(UserDbContext db) => _db = db;


        public async Task AddAsync(RefreshToken token)
        {
            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync();
        }


        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token);
        }


        public async Task RevokeAsync(RefreshToken token)
        {
            _db.RefreshTokens.Remove(token);
            await _db.SaveChangesAsync();
        }
    }
}
