using Domain.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;

namespace ApiSkyrimRP.Core
{
    public class ServersCacheService
    {
        public IImmutableDictionary<Guid, ServerInfo> ReadOnlyDictionary => servers.ToImmutableDictionary();
        private readonly ConcurrentDictionary<Guid, ServerInfo> servers;

        public ServersCacheService()
        {
            servers = new();
        }

        public void RefreshServers(DateTime now)
        {
            List<KeyValuePair<Guid, ServerInfo>> expiredTokens = servers.Where(x => x.Value.ExpireAt < now).ToList();
            foreach (KeyValuePair<Guid, ServerInfo> expiredToken in expiredTokens) servers.TryRemove(expiredToken.Key, out _);
        }

        public void RefreshServer(Guid key, ServerInfo info)
        {
            servers.AddOrUpdate(key, info, (_, _) => info);
        }
    }

    public class ServerInfo
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public ServerLanguages Language { get; set; }
        public ServerType Type { get; set; }
        public ServerFlags Flags { get; set; }

        [JsonIgnore]
        public DateTime ExpireAt { get; set; }
    }
}
