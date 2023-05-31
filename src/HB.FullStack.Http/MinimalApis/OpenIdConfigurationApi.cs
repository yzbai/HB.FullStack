using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Server.Identity;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;

namespace HB.FullStack.Server.WebLib.MinimalApis
{
    public static class OpenIdConfigurationApi
    {
        public static RouteGroupBuilder MapOpenIdConfigurationApi<TId>(this RouteGroupBuilder group)
        {
            group.MapGet("/", GetOpenIdConnectConfiguration<TId>).AllowAnonymous();
            group.MapGet("/Jwks", GetJwks<TId>).AllowAnonymous();

            return group;
        }

        private static IResult GetOpenIdConnectConfiguration<TId>(IIdentityService<TId> identityService)
        {
            return Results.Content(identityService.OpenIdConnectConfigurationString, "application/json");
        }

        private static IResult GetJwks<TId>(IIdentityService<TId> identityService)
        {
            return Results.Content(identityService.JsonWebKeySet, "application/json");
        }
    }
}
