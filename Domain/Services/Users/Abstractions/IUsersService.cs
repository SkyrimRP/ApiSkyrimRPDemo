using Domain.Entities;
using Domain.Services.Users.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services.Users.Abstractions
{
    public interface IUsersService
    {
        int GetPagesCount();
        Task<List<User>> GetListAsync(int offset = 0);
        Task<User> GetUserAsync(int id);
        Task<User> GetUserAsync(string email);
        Task<User> AddAsync(RegUserInfo info);
        Task<bool> VerifyPasswordAsync(LoginUserInfo info);
        Task EditAsync(int id, Func<User, bool> editable);
        Task DeleteAsync(int id);

    }
}
