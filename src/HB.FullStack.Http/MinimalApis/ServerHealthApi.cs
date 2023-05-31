using HB.FullStack.Common.Shared;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HB.FullStack.Server.WebLib.MinimalApis
{
    public static class ServerHealthApi
    {
        public static RouteGroupBuilder MapServerHealthApi(this RouteGroupBuilder group)
        {

            //TODO: implement AllowExpiredTokenAttribute
            //[AllowExpiredToken]

            group.MapGet("/", () =>
            {
                //TODO: 记得用缓存，锁，等等
                //TODO: 所有AllowAnonymous都要进行统一的安全管理
                return Results.Ok(new ServerHealthRes { ServerHealthy = ServerHealthy.UP });
            }).AllowAnonymous();
            return group;
        }
    }
}
