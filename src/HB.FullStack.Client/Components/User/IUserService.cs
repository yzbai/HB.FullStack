﻿/*
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

        Task<UserProfile?> UpdateUserProfileAsync(
            string? nickName,
            Gender? gender,
            DateOnly? birthDay,
            string? tmpAvatarFileFullPath);

        ///<summary>
        ///返回本地FullPath
        ///</summary>
        Task<string?> GetAvatarFileAsync(GetSetMode getMode = GetSetMode.Mixed);

        ///<summary>
        ///返回本地FullPath
        ///</summary>
        Task<string?> SetAvatarFileAsync(string avatarFullPath, GetSetMode setMode = GetSetMode.Mixed);

        Task<string?> GetNickNameAsync(GetSetMode getMode = GetSetMode.Mixed);

        #endregion

        Task LoginBySmsCodeAsync(string mobile, string smsCode, string audience);
    }
}