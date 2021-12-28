using Database;
using Domain.Entities;
using Domain.Services.Players.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Services.Players
{
    public class PlayersService : IPlayersService
    {
        public const int Count = 25;

        private readonly DatabaseContext ctx;

        public PlayersService(DatabaseContext context)
        {
            ctx = context;
        }

        public int GetPagesCount()
        {
            int elms = ctx.Users.Count();
            double elemOfPages = (double)elms / Count;
            return (int)Math.Ceiling(elemOfPages);
        }

        public Task<List<Player>> GetListAsync(int offset = 0)
        {
            return ctx.Players.OrderBy(o => o.Id).Skip(offset * Count).Take(Count).ToListAsync();
        }

        public Task<Player> GetAsync(int id)
        {
            return ctx.Players.FirstOrDefaultAsync(f => f.Id == id);
        }

        public Task<Player> GetAsync(Guid session)
        {
            return ctx.Players.FirstOrDefaultAsync(f => f.Session == session);
        }

        public async Task<Player> AddAsync(Player info)
        {
            ctx.Players.Add(info);
            await ctx.SaveChangesAsync();
            return info;
        }

        public async Task DeleteAsync(int id)
        {
            Player model = await GetAsync(id);
            if (model != null)
            {
                ctx.Players.Remove(model);
                await ctx.SaveChangesAsync();
            }
            else throw new Exception("Player not found");
        }

        public async Task EditAsync(int id, Action<Player> info)
        {
            Player model = await GetAsync(id);
            if (model != null)
            {
                info.Invoke(model);
                await ctx.SaveChangesAsync();
            }
            else throw new Exception("Player not found");
        }
    }
}
