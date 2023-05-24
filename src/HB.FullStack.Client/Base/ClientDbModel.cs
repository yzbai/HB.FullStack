using System;

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Base
{
    [PropertyTrackableObject]
    public abstract partial class ClientDbModel : DbModel<Guid>, IExpired
    {
        [TrackProperty]
        private long? _expiredAt;

        /// <summary>
        /// 改动时间，包括：
        /// 1. Update
        /// 2. Get from network
        /// </summary>
        //[TrackProperty]
        //private DateTimeOffset _lastTime = DateTimeOffset.UtcNow;
    }
}