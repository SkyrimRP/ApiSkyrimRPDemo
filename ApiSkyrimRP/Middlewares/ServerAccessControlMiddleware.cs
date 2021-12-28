using Domain.Entities;
using Domain.Services.Servers.Abstarctions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ApiSkyrimRP.Middlewares
{
    public class ServerAccessControlAttribute : Attribute { }

    public class ServerAccessControlMiddleware : IMiddleware
    {
        private readonly IServersService serversService;

        public ServerAccessControlMiddleware(IServersService servers)
        {
            serversService = servers;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Endpoint endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint;
            ServerAccessControlAttribute attribute = endpoint?.Metadata.GetMetadata<ServerAccessControlAttribute>();
            if (attribute != null)
            {
                if (context.Request.RouteValues.TryGetValue("ServerKey", out object obj) && obj is string serverId && !string.IsNullOrWhiteSpace(serverId))
                {
                    Server server = await serversService.GetAsync(Guid.Parse(serverId));
                    if (server != null)
                    {
                        await next.Invoke(context);
                        return;
                    }
                }
                await ReturnErrorResponse(context);
            }
            else
            {
                await next.Invoke(context);
            }
        }

        private Task ReturnErrorResponse(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            return Task.CompletedTask;
        }
    }
}
