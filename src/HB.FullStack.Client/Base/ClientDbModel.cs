using System;

using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.Client.Base
{
    [PropertyTrackableObject]
    public partial class ClientDbModel : DbModel2<Guid>
    {
        /// <summary>
        /// 改动时间，包括：
        /// 1. Update
        /// 2. Get from network
        /// </summary>
        [TrackProperty]
        private DateTimeOffset _lastTime = DateTimeOffset.UtcNow;

        public override Guid Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override bool Deleted { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public override string? LastUser { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}