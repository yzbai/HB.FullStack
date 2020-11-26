using HB.Component.Authorization.Entities;
using HB.FullStack.Database;

using System;
using System.Threading.Tasks;

namespace HB.Component.Authorization.Abstractions
{
    internal interface ISignInTokenBiz
    {
        Task<SignInToken> CreateAsync(string userGuid, string deviceId, DeviceInfos deviceInfos, string deviceVersion, /*string deviceAddress,*/ string ipAddress, TimeSpan expireTimeSpan, string lastUser, TransactionContext? transContext = null);


        Task DeleteByLogOffTypeAsync(string userGuid, DeviceIdiom currentIdiom, LogOffType logOffType, string lastUser);


        Task DeleteAsync(string signInTokenGuid, string lastUser);

        Task DeleteByUserGuidAsync(string userGuid, string lastUser);


        Task<SignInToken?> GetAsync(string? signInTokenGuid, string? refreshToken, string deviceId, string? userGuid);


        Task UpdateAsync(SignInToken signInToken, string lastUser);
    }
}
