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
    public class UserProfileRes : SharedResource
    {
        public Guid Id { get; set; }

        [NoEmptyGuid]
        public Guid UserId { get; set; }

        [NickName(CanBeNull = false)]
        public string NickName { get; set; } = null!;

        public Gender? Gender { get; set; }

        public DateOnly? BirthDay { get; set; }

        public string? AvatarFileName { get; set; }
    }
}