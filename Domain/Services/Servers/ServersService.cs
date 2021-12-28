using Database;
using Domain.Entities;
using Domain.Services.Servers.Abstarctions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Services.Servers
{
    public class ServersService : IServersService
    {
        public const int Count = 25;

        private readonly DatabaseContext ctx;

        public ServersService(DatabaseContext context)
        {
            ctx = context;
        }

        public int GetPagesCount()
        {
            int elms = ctx.Users.Count();
            double elemOfPages = (double)elms / Count;
            return (int)Math.Ceiling(elemOfPages);
        }

        public Task<List<Server>> GetListAsync(int offset = 0)
        {
            return ctx.Servers.OrderBy(o => o.Id).Skip(offset * Count).Take(Count).ToListAsync();
        }

        public Task<Server> GetAsync(int id)
        {
            return ctx.Servers.FirstOrDefaultAsync(f => f.Id == id);
        }

        public Task<Server> GetAsync(Guid key)
        {
            return ctx.Servers.FirstOrDefaultAsync(f => f.Key == key);
        }

        public async Task<Server> AddAsync(Server info)
        {
            ctx.Servers.Add(info);
            await ctx.SaveChangesAsync();
            return info;
        }

        public async Task DeleteAsync(int id)
        {
            Server server = await GetAsync(id);
            if (server != null)
            {
                ctx.Servers.Remove(server);
                await ctx.SaveChangesAsync();
            }
            else throw new Exception("Server not found");
        }

        public async Task EditAsync(Server info)
        {
            Server server = await GetAsync(info.Id);
            if (server != null)
            {
                server.Name = info.Name;
                server.Address = info.Address;
                server.Language = info.Language;
                server.Type = info.Type;
                server.Flags = info.Flags;
                await ctx.SaveChangesAsync();
            }
            else throw new Exception("Server not found");
        }
    }
}
