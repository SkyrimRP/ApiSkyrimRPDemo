using Database;
using Domain.Entities;
using Domain.Services.Users.Abstractions;
using Domain.Services.Users.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Services.Users
{
    public class UsersService : IUsersService
    {
        public const int Count = 25;

        private readonly DatabaseContext ctx;

        public UsersService(DatabaseContext context)
        {
            ctx = context;
        }

        public int GetPagesCount()
        {
            int elms = ctx.Users.Count();
            double elemOfPages = (double)elms / Count;
            return (int)Math.Ceiling(elemOfPages);
        }

        public Task<List<User>> GetListAsync(int offset = 0)
        {
            return ctx.Users.OrderBy(o => o.Id).Skip(offset * Count).Take(Count).Include(i => i.Roles).ToListAsync();
        }

        public Task<User> GetUserAsync(int id)
        {
            return ctx.Users.Include(i => i.Roles).Include(i => i.Players).FirstOrDefaultAsync(f => f.Id == id);
        }

        public Task<User> GetUserAsync(string email)
        {
            return ctx.Users.Include(i => i.Roles).FirstOrDefaultAsync(f => f.Email == email);
        }

        public async Task<User> AddAsync(RegUserInfo info)
        {
            User user = new()
            {
                Username = info.Username,
                Email = info.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(info.Password),
                Code = Guid.NewGuid()
            };
            ctx.Users.Add(user);
            await ctx.SaveChangesAsync();
            return user;
        }

        public async Task<bool> VerifyPasswordAsync(LoginUserInfo info)
        {
            User user = await GetUserAsync(info.Email);
            if (user != null)
                return BCrypt.Net.BCrypt.Verify(info.Password, user.Password);
            else throw new Exception("User not found");
        }

        public async Task DeleteAsync(int id)
        {
            User user = await GetUserAsync(id);
            if (user != null)
            {
                ctx.Users.Remove(user);
                await ctx.SaveChangesAsync();
            }
            else throw new Exception("User not found");
        }

        public async Task EditAsync(int id, Func<User, bool> editable)
        {
            User user = await GetUserAsync(id);
            if (user != null)
            {
                if (editable?.Invoke(user) == true) await ctx.SaveChangesAsync();
            }
            else throw new Exception("User not found");
        }
    }
}
