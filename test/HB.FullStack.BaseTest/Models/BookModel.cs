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
    public class Book2Model : DbModel2<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;

        public long Timestamp { get; set; }

        public override long Id { get; set; }

        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    

    [PropertyTrackableObject]
    public partial class Guid_BookModel_Timeless : DbModel2<Guid>
    {
        [TrackProperty]
        private string _name = default!;

        [TrackProperty]
        private double _price = default!;

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    [CacheModel]
    public class Book : DbModel2<long>, ITimestamp
    {
        [DbField]
        [CacheModelAltKey]
        public string Name { get; set; } = null!;

        [DbField]
        [CacheModelAltKey]
        public long BookID { get; set; }

        [DbField]
        public string? Publisher { get; set; }

        [DbField]
        public double Price { get; set; }

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class BookModel_Client : DbModel2<long>, ITimestamp
    {
        [DbField(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Book_Client : DbModel2<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = null!;

        [DbField]
        public long BookID { get; set; }

        [DbField]
        public string? Publisher { get; set; }

        [DbField]
        public double Price { get; set; }

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}