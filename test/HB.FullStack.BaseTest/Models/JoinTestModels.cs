/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Models
{
    public class A : DbModel2<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class B : DbModel2<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class AB : DbModel2<long>, ITimestamp
    {
        [DbField]
        public long AId { get; set; } = default!;

        [DbField]
        public long BId { get; set; } = default!;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class C : DbModel2<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public long AId { get; set; } = default!;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_A : DbModel2<Guid>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_B : DbModel2<Guid>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_AB : DbModel2<Guid>, ITimestamp
    {
        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_C : DbModel2<Guid>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public Guid Guid_AId { get; set; }

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}