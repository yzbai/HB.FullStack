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
    public class A : DbModel<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class B : DbModel<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class AB : DbModel<long>, ITimestamp
    {
        [DbField]
        [DbForeignKey(typeof(A),true)]
        public long AId { get; set; } = default!;

        [DbField]
        [DbForeignKey(typeof(B), true)]
        public long BId { get; set; } = default!;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class C : DbModel<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        [DbForeignKey(typeof(A), true)]
        public long AId { get; set; } = default!;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_A : DbModel<Guid>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_B : DbModel<Guid>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_AB : DbModel<Guid>, ITimestamp
    {
        [DbForeignKey(typeof(Guid_A), true)]
        public Guid Guid_AId { get; set; }

        [DbForeignKey(typeof(Guid_B), true)]
        public Guid Guid_BId { get; set; }
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_C : DbModel<Guid>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        [DbForeignKey(typeof(Guid_A), true)]
        public Guid Guid_AId { get; set; }

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}