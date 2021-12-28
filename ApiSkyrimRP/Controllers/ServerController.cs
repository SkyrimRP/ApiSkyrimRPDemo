using ApiSkyrimRP.Core;
using ApiSkyrimRP.Middlewares;
using Domain.Entities;
using Domain.Services.Players.Abstractions;
using Domain.Services.Servers.Abstarctions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ApiSkyrimRP.Controllers
{
    [Route("api/[controller]/{ServerKey}")]
    [ApiController]
    [ServerAccessControl]
    public class ServerController : ControllerBase
    {
        private readonly IServersService serversService;
        private readonly IPlayersService playersService;
        private readonly ServersCacheService serversCache;

        public ServerController(IServersService servers, IPlayersService players, ServersCacheService cacheService)
        {
            serversService = servers;
            playersService = players;
            serversCache = cacheService;
        }

        [HttpPost("servers/{address}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public async Task<IActionResult> Post([Required] Guid ServerKey, string address, [Required] PostRequest request)
        {
            Server server = await serversService.GetAsync(ServerKey);

            ServerInfo serverInfo = new()
            {
                Name = string.IsNullOrWhiteSpace(request.Name) ? server.Name : request.Name,
                Address = string.IsNullOrWhiteSpace(address) ? server.Address : address,
                Type = server.Type,
                Flags = server.Flags,
                Language = server.Language,
                ExpireAt = DateTime.Now.AddSeconds(15)
            };

            serversCache.RefreshServer(ServerKey, serverInfo);

            return Ok();
        }

        public class PostRequest
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }
            [JsonPropertyName("online")]
            public int Online { get; set; }
            [JsonPropertyName("maxPlayers")]
            public int MaxPlayers { get; set; }
        }

        [HttpPost("servers/{address}/sessions/{session}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public async Task<IActionResult> Get([Required] Guid ServerKey, string address, [Required] Guid session)
        {
            Server server = await serversService.GetAsync(ServerKey);

            Player player = await playersService.GetAsync(session);
            if (player == null) return NotFound();
            if (player.Session != session || player.ServerSession != server.Id) return NotFound();
            if (player.User.IsBlocked || player.LastUpdate <= DateTime.Now.AddMinutes(-10)) return NotFound();
            return new JsonResult(new { user = new { id = player.Id } });
        }

    }
}
