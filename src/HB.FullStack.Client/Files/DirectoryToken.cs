/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Client.Base;
using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Client.Files
{
    [SyncSetting(allowOfflineRead: false, allowOfflineAdd: false, allowOfflineDelete: false, allowOfflineUpdate: false)]
    public partial class DirectoryToken : ClientDbModel
    {
        public override Guid Id { get; set; }

        public override bool Deleted { get; set; }

        public override string? LastUser { get; set; }

        [TrackProperty]
        private Guid _userId;

        [TrackProperty]
        private string _securityToken = null!;

        [TrackProperty]
        private string _accessKeyId = null!;

        [TrackProperty]
        private string _accessKeySecret = null!;

        [TrackProperty]
        private string _directoryPermissionName = null!;

        [TrackProperty]
        private bool _readOnly;
    }
}