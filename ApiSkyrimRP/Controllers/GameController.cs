using ApiSkyrimRP.Core;
using Domain.Entities;
using Domain.Services.Players.Abstractions;
using Domain.Services.Servers.Abstarctions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ApiSkyrimRP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private readonly IServersService serversService;
        private readonly IPlayersService playersService;
        private readonly ServersCacheService serversCache;

        public GameController(IServersService servers, IPlayersService players, ServersCacheService cacheService)
        {
            serversService = servers;
            playersService = players;
            serversCache = cacheService;
        }

        [HttpPost("Play/{ServerId}/{PlayerId}")]
        [Authorize]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [Produces("application/json")]
        public async Task<IActionResult> Play([Required] int ServerId, [Required] int PlayerId)
        {
            Server server = await serversService.GetAsync(ServerId);
            if (server == null) return NotFound();

            Claim uid = User.Claims.FirstOrDefault(f => f.Type == "UID");
            if (int.TryParse(uid.Value, out int id))
            {
                bool success = true;
                Guid session = Guid.NewGuid();
                await playersService.EditAsync(PlayerId, player =>
                {
                    if (player.UserId != id) { success = false; return; }
                    player.Session = session;
                    player.ServerSession = server.Id;
                    player.LastUpdate = DateTime.UtcNow;
                });

                if (!success) return BadRequest("Player not found.");

                return Ok(new { Session = session.ToString() });
            }
            else { return StatusCode(520); }
        }

        [HttpGet("Servers")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [Produces("application/json")]
        public async Task<IEnumerable<ServerInfo>> Servers()
        {
            return serversCache.ReadOnlyDictionary.Values;
        }
    }
}
