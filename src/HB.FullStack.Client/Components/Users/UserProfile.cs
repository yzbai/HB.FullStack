/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Client.Base;
using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Common.Shared;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Components.Users
{
    [SyncSetting(allowOfflineRead: false, allowOfflineAdd: false, allowOfflineDelete: false, allowOfflineUpdate: false)]
    [PropertyTrackableObject]
    public partial class UserProfile : DbModel<Guid>, IExpired
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

        [TrackProperty]
        private long? _expiredAt;

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }
}