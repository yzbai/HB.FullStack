using System;
using System.Collections.Generic;

using HB.FullStack.Database;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    [DbModel(DbSchema_Mysql)]
    public class PublisherModel3 : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public int Integer { get; set; } = 999;

        [DbModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DbModelProperty]
        public string Name { get; set; } = "Name";

        [DbModelProperty]
        public string Name2 { get; set; } = "Name2";

        [DbModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }
    [DbModel(DbSchema_Mysql)]
    public class Guid_PublisherModel3 : TimestampGuidDbModel
    {
        [DbModelProperty]
        public int Integer { get; set; } = 999;

        [DbModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DbModelProperty]
        public string Name { get; set; } = "Name";

        [DbModelProperty]
        public string Name2 { get; set; } = "Name2";

        [DbModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }
    [DbModel(DbSchema_Mysql)]
    public class PublisherModel2 : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public int Integer { get; set; } = 999;

        [DbModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [DbModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [DbModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DbModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DbModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DbModelProperty]
        public int? Number { get; set; } = 12121221;

        [DbModelProperty]
        public int? Number1 { get; set; }

        [DbModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DbModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }
    [DbModel(DbSchema_Mysql)]
    public class Guid_PublisherModel2 : TimestampGuidDbModel
    {
        [DbModelProperty]
        public int Integer { get; set; } = 999;

        [DbModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [DbModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [DbModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DbModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DbModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DbModelProperty]
        public int? Number { get; set; } = 12121221;

        [DbModelProperty]
        public int? Number1 { get; set; }

        [DbModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DbModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }
    [DbModel(DbSchema_Mysql)]
    public class PublisherModel : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public IList<string> Books { get; set; } = default!;

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DbModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DbModelProperty]
        public PublisherType Type { get; set; }

        [DbModelProperty]
        public int? Number { get; set; }

        [DbModelProperty]
        public int? Number1 { get; set; } = 111;

        [DbModelProperty]
        public PublisherType? Type2 { get; set; }

        [DbModelProperty]
        public PublisherType? Type3 { get; set; }

        [DbModelProperty]
        public string? Name2 { get; set; }

        [DbModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DbModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DbModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }
    [DbModel(DbSchema_Mysql)]
    public class Guid_PublisherModel : TimestampGuidDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public IList<string> Books { get; set; } = default!;

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DbModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DbModelProperty]
        public PublisherType Type { get; set; }

        [DbModelProperty]
        public int? Number { get; set; }

        [DbModelProperty]
        public int? Number1 { get; set; } = 111;

        [DbModelProperty]
        public PublisherType? Type2 { get; set; }

        [DbModelProperty]
        public PublisherType? Type3 { get; set; }

        [DbModelProperty]
        public string? Name2 { get; set; }

        [DbModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DbModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DbModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
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
    [DbModel(DbSchema_Mysql)]
    public class PublisherModel3_Client : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public int Integer { get; set; } = 999;

        [DbModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DbModelProperty]
        public string Name { get; set; } = "Name";

        [DbModelProperty]
        public string Name2 { get; set; } = "Name2";

        [DbModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }
    [DbModel(DbSchema_Mysql)]
    public class PublisherModel2_Client : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public int Integer { get; set; } = 999;

        [DbModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [DbModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [DbModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DbModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DbModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DbModelProperty]
        public int? Number { get; set; } = 12121221;

        [DbModelProperty]
        public int? Number1 { get; set; }

        [DbModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DbModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }
    [DbModel(DbSchema_Mysql)]
    public class PublisherModel_Client : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public IList<string> Books { get; set; } = default!;

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DbModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DbModelProperty]
        public PublisherType Type { get; set; }

        [DbModelProperty]
        public int? Number { get; set; }

        [DbModelProperty]
        public int? Number1 { get; set; } = 111;

        [DbModelProperty]
        public PublisherType? Type2 { get; set; }

        [DbModelProperty]
        public PublisherType? Type3 { get; set; }

        [DbModelProperty]
        public string? Name2 { get; set; }

        [DbModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DbModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DbModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }
    [DbModel(DbSchema_Mysql)]
    public class Guid_PublisherModel_Client : TimestampGuidDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public IList<string> Books { get; set; } = default!;

        [DbModelProperty(Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DbModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DbModelProperty]
        public PublisherType Type { get; set; }

        [DbModelProperty]
        public int? Number { get; set; }

        [DbModelProperty]
        public int? Number1 { get; set; } = 111;

        [DbModelProperty]
        public PublisherType? Type2 { get; set; }

        [DbModelProperty]
        public PublisherType? Type3 { get; set; }

        [DbModelProperty]
        public string? Name2 { get; set; }

        [DbModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DbModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DbModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }
}