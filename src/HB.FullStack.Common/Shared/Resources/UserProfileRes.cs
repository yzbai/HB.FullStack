/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Models;

namespace HB.FullStack.Common.Shared
{
    //public interface IUserProfileRes : ISharedResource
    //{
    //    object UserId { get; set; }

    //    string? AvatarFileName { get; set; }

    //    DateOnly? BirthDay { get; set; }

    //    Gender? Gender { get; set; }

    //    string NickName { get; set; }
    //}

    public class UserProfileRes<TId> : SharedResource2<TId>
    {
        public TId UserId { get; set; } = default!;

        [NickName(CanBeNull = false)]
        public string NickName { get; set; } = null!;

        public Gender? Gender { get; set; }

        public DateOnly? BirthDay { get; set; }

        public string? AvatarFileName { get; set; }
        public override TId? Id {get;set;}
        public override long? ExpiredAt {get;set;}
    }
}