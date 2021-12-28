using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services.Servers.Abstarctions
{
    public interface IServersService
    {
        int GetPagesCount();
        Task<List<Server>> GetListAsync(int offset = 0);
        Task<Server> GetAsync(int id);
        public Task<Server> GetAsync(Guid key);
        Task<Server> AddAsync(Server info);
        Task EditAsync(Server info);
        Task DeleteAsync(int id);
    }
}
