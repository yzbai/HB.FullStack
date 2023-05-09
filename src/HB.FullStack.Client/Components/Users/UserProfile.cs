/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.Base;
using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common.Shared;

namespace HB.FullStack.Client.Components.Users
{
    [ClientModelSetting(expiryTimeType: ExpiryTimeType.Tiny, allowOfflineRead: false, allowOfflineAdd: false, allowOfflineDelete: false, allowOfflineUpdate: false)]
    public partial class UserProfile : ClientDbModel
    {
        [NoEmptyGuid]
        [TrackProperty]
        [AddtionalProperty]
        private Guid _userId;

        [TrackProperty]
        private string? _level;

        [NickName(CanBeNull = false)]
        [TrackProperty]
        private string _nickName = null!;

        [TrackProperty]
        private Gender? _gender;

        [TrackProperty]
        private DateOnly? _birthDay;

        [TrackProperty]
        private string? _avatarFileName;

    }
}