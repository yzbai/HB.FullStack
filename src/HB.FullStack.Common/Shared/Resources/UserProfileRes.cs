/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common.Models;
using HB.FullStack.Common.Shared.Context;

namespace HB.FullStack.Common.Shared.Resources
{
    public class UserProfileRes : ApiResource
    {
        public Guid Id { get; set; }

        [NoEmptyGuid]
        public Guid UserId { get; set; }

        public string? Level { get; set; }

        [NickName(CanBeNull = false)]
        public string NickName { get; set; } = null!;

        public Gender? Gender { get; set; }

        public DateOnly? BirthDay { get; set; }

        public string? AvatarFileName { get; set; }
    }
}