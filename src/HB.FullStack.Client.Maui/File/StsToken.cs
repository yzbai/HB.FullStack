using System;

using HB.FullStack.Client.ClientModels;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Maui.File
{
    [ClientModel(
      expirySeconds: int.MaxValue, //由StsToken.ExpirationAt业务逻辑决定
      allowOfflineRead: false,
      allowOfflineWrite: false)]
    public partial class StsToken : ClientDbModel
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
        private static TimeSpan _gapTime = TimeSpan.FromMinutes(1);

        public static bool IsExpired(this StsToken token)
        {
            return token.ExpirationAt - TimeUtil.UtcNow < _gapTime;
        }
    }
}