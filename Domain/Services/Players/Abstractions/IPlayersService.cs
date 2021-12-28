using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Services.Players.Abstractions
{
    public interface IPlayersService
    {
        int GetPagesCount();
        Task<List<Player>> GetListAsync(int offset = 0);
        Task<Player> GetAsync(int id);
        public Task<Player> GetAsync(Guid session);
        Task<Player> AddAsync(Player info);
        Task EditAsync(int id, Action<Player> editable);
        Task DeleteAsync(int id);
    }
}
