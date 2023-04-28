using System;
using System.Collections.Generic;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.BaseTest.Data.MySqls;
using HB.FullStack.Database.Config;

namespace HB.FullStack.BaseTest.Data.Sqlites
{
    [DbTable(DbSchema_Sqlite)]
    public class PublisherModel3 : TimestampFlackIdDbModel
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
    }
    [DbTable(DbSchema_Sqlite)]
    public class Guid_PublisherModel3 : TimestampGuidDbModel
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
    }
    [DbTable(DbSchema_Sqlite)]
    public class PublisherModel2 : TimestampFlackIdDbModel
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
    }
    [DbTable(DbSchema_Sqlite)]
    public class Guid_PublisherModel2 : TimestampGuidDbModel
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
    }
    [DbTable(DbSchema_Sqlite)]
    public class PublisherModel : TimestampFlackIdDbModel
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
    }
    [DbTable(DbSchema_Sqlite)]
    public class Guid_PublisherModel : TimestampGuidDbModel
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
    }

    [DbTable(DbSchema_Sqlite)]
    public class PublisherModel3_Client : TimestampFlackIdDbModel
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
    }
    [DbTable(DbSchema_Sqlite)]
    public class PublisherModel2_Client : TimestampFlackIdDbModel
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
    }
    [DbTable(DbSchema_Sqlite)]
    public class PublisherModel_Client : TimestampFlackIdDbModel
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
    }
    [DbTable(DbSchema_Sqlite)]
    public class Guid_PublisherModel_Client : TimestampGuidDbModel
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
    }
}