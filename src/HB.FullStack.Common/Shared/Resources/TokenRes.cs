/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    public interface ITokenRes
    {
        string AccessToken { get; set; }
        string? Email { get; set; }
        bool EmailConfirmed { get; set; }
        long? ExpiredAt { get; set; }
        Guid? Id { get; set; }
        string? LoginName { get; set; }
        string? Mobile { get; set; }
        bool MobileConfirmed { get; set; }
        string RefreshToken { get; set; }
        bool TwoFactorEnabled { get; set; }
        Guid UserId { get; set; }
        string? UserLevel { get; set; }
    }

    /// <summary>
    /// 可能存在多个Endpoint，即不同的Endpoint使用不同站点的Token
    /// </summary>
    public class TokenRes2 : SharedResource2<Guid?>, ITokenRes
    {
        public Guid UserId { get; set; }

        public string? UserLevel { get; set; }

        public string? Mobile { get; set; }

        public string? LoginName { get; set; }

        public string? Email { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool MobileConfirmed { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public string AccessToken { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        //public override Guid? Id { get; set; }

        public override long? ExpiredAt { get; set; }
        public override Guid? Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}