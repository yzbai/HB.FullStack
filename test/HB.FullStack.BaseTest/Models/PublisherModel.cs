/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System.Collections.Generic;

using HB.FullStack.Common;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Models
{
    public class PublisherModel3 : DbModel2<long>, ITimestamp
    {
        [DbField]
        public int Integer { get; set; } = 999;

        [DbField]
        public float Float { get; set; } = 1.9877f;

        [DbField]
        public string Name { get; set; } = "Name";

        [DbField]
        public string Name2 { get; set; } = "Name2";

        [DbField]
        public string Name3 { get; set; } = "xxxx";

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_PublisherModel3 : DbModel2<Guid>, ITimestamp
    {
        [DbField]
        public int Integer { get; set; } = 999;

        [DbField]
        public float Float { get; set; } = 1.9877f;

        [DbField]
        public string Name { get; set; } = "Name";

        [DbField]
        public string Name2 { get; set; } = "Name2";

        [DbField]
        public string Name3 { get; set; } = "xxxx";

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class PublisherModel2 : DbModel2<long>, ITimestamp
    {
        [DbField]
        public int Integer { get; set; } = 999;

        [DbField]
        public float Float { get; set; } = 1.9877f;

        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public string? Name2 { get; set; } = "Name2";

        [DbField]
        public string? Name3 { get; set; } //= "xxxx";

        [DbField]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DbField]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DbField]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DbField]
        public int? Number { get; set; } = 12121221;

        [DbField]
        public int? Number1 { get; set; }

        [DbField]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DbField]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_PublisherModel2 : DbModel2<Guid>, ITimestamp
    {
        [DbField]
        public int Integer { get; set; } = 999;

        [DbField]
        public float Float { get; set; } = 1.9877f;

        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public string? Name2 { get; set; } = "Name2";

        [DbField]
        public string? Name3 { get; set; } //= "xxxx";

        [DbField]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DbField]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DbField]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DbField]
        public int? Number { get; set; } = 12121221;

        [DbField]
        public int? Number1 { get; set; }

        [DbField]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DbField]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class PublisherModel : DbModel2<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public IList<string> Books { get; set; } = default!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DbField(MaxLength = DbSchema.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DbField]
        public PublisherType Type { get; set; }

        [DbField]
        public int? Number { get; set; }

        [DbField]
        public int? Number1 { get; set; } = 111;

        [DbField]
        public PublisherType? Type2 { get; set; }

        [DbField]
        public PublisherType? Type3 { get; set; }

        [DbField]
        public string? Name2 { get; set; }

        [DbField]
        public string? Name3 { get; set; } = "xxxx";

        [DbField]
        public DateTimeOffset? DDD { get; set; }

        [DbField]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_PublisherModel : DbModel2<Guid>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public IList<string> Books { get; set; } = default!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DbField(MaxLength = DbSchema.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DbField]
        public PublisherType Type { get; set; }

        [DbField]
        public int? Number { get; set; }

        [DbField]
        public int? Number1 { get; set; } = 111;

        [DbField]
        public PublisherType? Type2 { get; set; }

        [DbField]
        public PublisherType? Type3 { get; set; }

        [DbField]
        public string? Name2 { get; set; }

        [DbField]
        public string? Name3 { get; set; } = "xxxx";

        [DbField]
        public DateTimeOffset? DDD { get; set; }

        [DbField]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public enum PublisherType
    {
        Online,
        Big,
        Small
    }

    public class Author
    {
        public string Name { get; set; } = default!;

        public string Mobile { get; set; } = default!;
    }

    public class PublisherModel3_Client : DbModel2<long>, ITimestamp
    {
        [DbField]
        public int Integer { get; set; } = 999;

        [DbField]
        public float Float { get; set; } = 1.9877f;

        [DbField]
        public string Name { get; set; } = "Name";

        [DbField]
        public string Name2 { get; set; } = "Name2";

        [DbField]
        public string Name3 { get; set; } = "xxxx";

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class PublisherModel2_Client : DbModel2<long>, ITimestamp
    {
        [DbField]
        public int Integer { get; set; } = 999;

        [DbField]
        public float Float { get; set; } = 1.9877f;

        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public string? Name2 { get; set; } = "Name2";

        [DbField]
        public string? Name3 { get; set; } //= "xxxx";

        [DbField]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DbField]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DbField]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DbField]
        public int? Number { get; set; } = 12121221;

        [DbField]
        public int? Number1 { get; set; }

        [DbField]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DbField]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class PublisherModel_Client : DbModel2<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public IList<string> Books { get; set; } = default!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DbField(MaxLength = DbSchema.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DbField]
        public PublisherType Type { get; set; }

        [DbField]
        public int? Number { get; set; }

        [DbField]
        public int? Number1 { get; set; } = 111;

        [DbField]
        public PublisherType? Type2 { get; set; }

        [DbField]
        public PublisherType? Type3 { get; set; }

        [DbField]
        public string? Name2 { get; set; }

        [DbField]
        public string? Name3 { get; set; } = "xxxx";

        [DbField]
        public DateTimeOffset? DDD { get; set; }

        [DbField]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    public class Guid_PublisherModel_Client : DbModel2<Guid>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public IList<string> Books { get; set; } = default!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DbField(MaxLength = DbSchema.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DbField]
        public PublisherType Type { get; set; }

        [DbField]
        public int? Number { get; set; }

        [DbField]
        public int? Number1 { get; set; } = 111;

        [DbField]
        public PublisherType? Type2 { get; set; }

        [DbField]
        public PublisherType? Type3 { get; set; }

        [DbField]
        public string? Name2 { get; set; }

        [DbField]
        public string? Name3 { get; set; } = "xxxx";

        [DbField]
        public DateTimeOffset? DDD { get; set; }

        [DbField]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }
}