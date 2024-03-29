﻿/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared.Resources
{
    /// <summary>
    /// 可能存在多个Endpoint，即不同的Endpoint使用不同站点的Token
    /// </summary>
    public class TokenRes : ApiResource
    {
        public Guid UserId { get; set; }

        public string? Mobile { get; set; }

        public string? LoginName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool MobileConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTimeOffset TokenCreatedTime { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        //protected override int GetChildHashCode()
        //{
        //    return HashCode.Combine(UserId, Mobile, LoginName, LoginName, Email, TokenCreatedTime, AccessToken, RefreshToken);
        //}
    }
}