using HB.Component.Authorization.Entities;
using HB.Framework.Database;
using System;
using System.Threading.Tasks;

namespace HB.Component.Authorization.Abstractions
{
    internal interface ISignInTokenBiz
    {
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task<SignInToken> CreateAsync(string userGuid, string deviceId, DeviceInfos deviceInfos, string deviceVersion, /*string deviceAddress,*/ string ipAddress, TimeSpan expireTimeSpan, string lastUser, TransactionContext? transContext = null);


        /// <exception cref="System.ArgumentException"></exception>
        /// <exception cref="DatabaseException"></exception>
        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        Task DeleteByLogOffTypeAsync(string userGuid, DeviceIdiom currentIdiom, LogOffType logOffType, string lastUser);


        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task DeleteAsync(string signInTokenGuid, string lastUser);

        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task DeleteByUserGuidAsync(string userGuid, string lastUser);


        Task<SignInToken?> GetAsync(string? signInTokenGuid, string? refreshToken, string deviceId, string? userGuid);


        /// <exception cref="HB.Framework.Common.ValidateErrorException"></exception>
        /// <exception cref="DatabaseException"></exception>
        Task UpdateAsync(SignInToken signInToken, string lastUser);
    }
}
