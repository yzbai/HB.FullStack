/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Client.Base;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Context;

using System;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Components.User
{
    public interface IUserService
    {
        #region Profile

        Task<bool> NeedRegisterProfileAsync();

        Task UpdateUserProfileAsync(string? nickName, Gender? gender, DateTimeOffset? birthDay, string? avatarFileName);

        Task<string?> GetAvatarFileAsync(Guid userId, RepoGetMode getMode = RepoGetMode.Mixed);

        Task<string> SetAvatarFileAsync(string avatarFullPath);

        Task<string?> GetNickNameAsync(Guid userId, RepoGetMode getMode = RepoGetMode.Mixed);

        #endregion

        Task LoginBySmsCodeAsync(string mobile, string smsCode, DeviceInfos deviceInfos);
    }
}