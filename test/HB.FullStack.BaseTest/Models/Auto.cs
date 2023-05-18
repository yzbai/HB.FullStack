/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Models
{
    public class AutoIdBTTimestamp : DbModel2<long>, ITimestamp
    {
        public string Name { get; set; } = SecurityUtil.CreateRandomString(10);

        public int Age { get; set; } = 77;

        public long Timestamp { get; set; }

        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }

        public override bool Deleted { get; set; }

        public override string? LastUser { get; set; }
    }

    [PropertyTrackableObject]
    public partial class AutoIdBT : DbModel2<long>
    {
        [TrackProperty]
        private string _name = SecurityUtil.CreateRandomString(10);

        [TrackProperty]
        private int _age = 66;

        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }

        public override bool Deleted { get; set; }

        public override string? LastUser { get; set; }
    }
}