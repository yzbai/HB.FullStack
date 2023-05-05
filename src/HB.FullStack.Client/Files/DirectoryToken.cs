/*
 * Author：Yuzhao Bai
 * Email: yuzhaobai@outlook.com
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Client.Base;
using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Client.Files
{
    [ClientModelSetting(expiryTimeType: ExpiryTimeType.NonExpiry, //由StsToken.ExpirationAt业务逻辑决定
      allowOfflineRead: false, allowOfflineAdd: false, allowOfflineDelete: false, allowOfflineUpdate: false)]
    public partial class DirectoryToken : ClientDbModel
    {
        [TrackProperty]
        private Guid _userId;

        [TrackProperty]
        private string _securityToken = null!;

        [TrackProperty]
        private string _accessKeyId = null!;

        [TrackProperty]
        private string _accessKeySecret = null!;

        [TrackProperty]
        private DateTimeOffset _expirationAt;

        [TrackProperty]
        private string _directoryPermissionName = null!;

        [TrackProperty]
        private bool _readOnly;
    }

    public static class StsTokenExtensions
    {
        private static readonly TimeSpan _gapTime = TimeSpan.FromMinutes(1);

        public static bool IsExpired(this DirectoryToken token)
        {
            return token.ExpirationAt - TimeUtil.UtcNow < _gapTime;
        }
    }
}