using System;
using System.Collections.Generic;

using HB.FullStack.Database;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.DBModels;

namespace HB.FullStack.DatabaseTests
{
    public class PublisherModel3 : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        public int Integer { get; set; } = 999;

        [DBModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DBModelProperty]
        public string Name { get; set; } = "Name";

        [DBModelProperty]
        public string Name2 { get; set; } = "Name2";

        [DBModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    public class Guid_PublisherModel3 : TimestampGuidDBModel
    {
        [DBModelProperty]
        public int Integer { get; set; } = 999;

        [DBModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DBModelProperty]
        public string Name { get; set; } = "Name";

        [DBModelProperty]
        public string Name2 { get; set; } = "Name2";

        [DBModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    public class PublisherModel2 : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        public int Integer { get; set; } = 999;

        [DBModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [DBModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [DBModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DBModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DBModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DBModelProperty]
        public int? Number { get; set; } = 12121221;

        [DBModelProperty]
        public int? Number1 { get; set; }

        [DBModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DBModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }

    public class Guid_PublisherModel2 : TimestampGuidDBModel
    {
        [DBModelProperty]
        public int Integer { get; set; } = 999;

        [DBModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [DBModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [DBModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DBModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DBModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DBModelProperty]
        public int? Number { get; set; } = 12121221;

        [DBModelProperty]
        public int? Number1 { get; set; }

        [DBModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DBModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }

    public class PublisherModel : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [DBModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DBModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DBModelProperty]
        public PublisherType Type { get; set; }

        [DBModelProperty]
        public int? Number { get; set; }

        [DBModelProperty]
        public int? Number1 { get; set; } = 111;

        [DBModelProperty]
        public PublisherType? Type2 { get; set; }

        [DBModelProperty]
        public PublisherType? Type3 { get; set; }

        [DBModelProperty]
        public string? Name2 { get; set; }

        [DBModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DBModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DBModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }

    public class Guid_PublisherModel : TimestampGuidDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [DBModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DBModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DBModelProperty]
        public PublisherType Type { get; set; }

        [DBModelProperty]
        public int? Number { get; set; }

        [DBModelProperty]
        public int? Number1 { get; set; } = 111;

        [DBModelProperty]
        public PublisherType? Type2 { get; set; }

        [DBModelProperty]
        public PublisherType? Type3 { get; set; }

        [DBModelProperty]
        public string? Name2 { get; set; }

        [DBModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DBModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DBModelProperty]
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

    public class PublisherModel3_Client : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        public int Integer { get; set; } = 999;

        [DBModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DBModelProperty]
        public string Name { get; set; } = "Name";

        [DBModelProperty]
        public string Name2 { get; set; } = "Name2";

        [DBModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    public class PublisherModel2_Client : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        public int Integer { get; set; } = 999;

        [DBModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [DBModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [DBModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DBModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DBModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DBModelProperty]
        public int? Number { get; set; } = 12121221;

        [DBModelProperty]
        public int? Number1 { get; set; }

        [DBModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DBModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }

    public class PublisherModel_Client : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [DBModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DBModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DBModelProperty]
        public PublisherType Type { get; set; }

        [DBModelProperty]
        public int? Number { get; set; }

        [DBModelProperty]
        public int? Number1 { get; set; } = 111;

        [DBModelProperty]
        public PublisherType? Type2 { get; set; }

        [DBModelProperty]
        public PublisherType? Type3 { get; set; }

        [DBModelProperty]
        public string? Name2 { get; set; }

        [DBModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DBModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DBModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }

    public class Guid_PublisherModel_Client : TimestampGuidDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [DBModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DBModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DBModelProperty]
        public PublisherType Type { get; set; }

        [DBModelProperty]
        public int? Number { get; set; }

        [DBModelProperty]
        public int? Number1 { get; set; } = 111;

        [DBModelProperty]
        public PublisherType? Type2 { get; set; }

        [DBModelProperty]
        public PublisherType? Type3 { get; set; }

        [DBModelProperty]
        public string? Name2 { get; set; }

        [DBModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DBModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DBModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }
}