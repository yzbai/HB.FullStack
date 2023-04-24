/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Client.Base;
using HB.FullStack.Common.Files;
using HB.FullStack.Common.Shared;
using HB.FullStack.Common.Shared.Context;
using HB.FullStack.Common.Shared.Resources;

using System;
using System.Threading.Tasks;

namespace HB.FullStack.Client.Components.Users
{
    public interface IUserProfileService
    {
        #region Profile

        Task<UserProfile?> UpdateUserProfileAsync(
            string? nickName,
            Gender? gender,
            DateOnly? birthDay,
            string? tmpAvatarFileFullPath);

        ///<summary>
        ///返回本地FullPath
        ///</summary>
        Task<(Directory2, string?)> GetAvatarFileAsync(GetSetMode getMode = GetSetMode.Mixed);

        ///<summary>
        ///返回本地FullPath
        ///</summary>
        Task<string?> SaveAvatarFileAsync(string avatarFullPath, GetSetMode setMode = GetSetMode.Mixed);

        Task<string?> GetNickNameAsync(GetSetMode getMode = GetSetMode.Mixed);

        #endregion
    }
}