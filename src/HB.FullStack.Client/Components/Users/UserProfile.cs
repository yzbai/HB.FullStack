/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.Base;
using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.Shared.Context;

namespace HB.FullStack.Client.Components.Users
{
    [ClientModelSetting(expiryTimeType: ExpiryTimeType.Tiny, allowOfflineRead: false, allowOfflineAdd: false, allowOfflineDelete: false, allowOfflineUpdate: false)]
    public class UserProfile : ClientDbModel
    {
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