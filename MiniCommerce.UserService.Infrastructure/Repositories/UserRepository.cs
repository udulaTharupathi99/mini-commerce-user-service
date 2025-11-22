using Microsoft.EntityFrameworkCore;
using MiniCommerce.UserService.Application.Interfaces;
using MiniCommerce.UserService.Domain.Entities;
using MiniCommerce.UserService.Infrastructure.Data;

namespace MiniCommerce.UserService.Infrastructure.Repositories
{
    public class UserRepository : IUserService
    {
        private readonly UserDbContext _db;
        public UserRepository(UserDbContext db) => _db = db;


        public async Task AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }


        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        }


        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }


        public async Task UpdateAsync(User user)
        {
            user.UpdatedAt = DateTime.UtcNow;
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}
