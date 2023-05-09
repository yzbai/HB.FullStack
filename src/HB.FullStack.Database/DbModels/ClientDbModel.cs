using System;

using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Database.DbModels
{
    [PropertyTrackableObject]
    public partial class ClientDbModel : TimelessGuidDbModel
    {

        /// <summary>
        /// 改动时间，包括：
        /// 1. Update
        /// 2. Get from network
        /// </summary>
        [TrackProperty]
        private DateTimeOffset _lastTime = DateTimeOffset.UtcNow;

    }
}
