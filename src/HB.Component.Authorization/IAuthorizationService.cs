using HB.Component.Authorization.Abstractions;
using HB.Component.Identity.Entities;

using Microsoft.IdentityModel.Tokens;

using System;
using System.Threading.Tasks;

namespace HB.Component.Authorization
{
    public interface IAuthorizationService
    {
        Task<SignInResult> SignInAsync(SignInContext context, string lastUser);

        Task SignOutAsync(string signInTokenGuid, string lastUser);

        Task SignOutAsync(string userGuid, DeviceIdiom idiom, LogOffType logOffType, string lastUser);

        Task<string> RefreshAccessTokenAsync(RefreshContext context, string lastUser);

        JsonWebKeySet JsonWebKeySet { get; }
    }
}