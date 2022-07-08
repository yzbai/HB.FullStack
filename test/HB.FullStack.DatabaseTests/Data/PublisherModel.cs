using HB.FullStack.Common;
using HB.FullStack.Database;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.DatabaseModels;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.FullStack.DatabaseTests
{
    public class PublisherModel3 : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public int Integer { get; set; } = 999;

        [DatabaseModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DatabaseModelProperty]
        public string Name { get; set; } = "Name";

        [DatabaseModelProperty]
        public string Name2 { get; set; } = "Name2";

        [DatabaseModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    public class Guid_PublisherModel3 : GuidDatabaseModel
    {
        [DatabaseModelProperty]
        public int Integer { get; set; } = 999;

        [DatabaseModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DatabaseModelProperty]
        public string Name { get; set; } = "Name";

        [DatabaseModelProperty]
        public string Name2 { get; set; } = "Name2";

        [DatabaseModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    public class PublisherModel2 : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public int Integer { get; set; } = 999;

        [DatabaseModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [DatabaseModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [DatabaseModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DatabaseModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DatabaseModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DatabaseModelProperty]
        public int? Number { get; set; } = 12121221;

        [DatabaseModelProperty]
        public int? Number1 { get; set; }

        [DatabaseModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DatabaseModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }

    public class Guid_PublisherModel2 : GuidDatabaseModel
    {
        [DatabaseModelProperty]
        public int Integer { get; set; } = 999;

        [DatabaseModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [DatabaseModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [DatabaseModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DatabaseModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DatabaseModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DatabaseModelProperty]
        public int? Number { get; set; } = 12121221;

        [DatabaseModelProperty]
        public int? Number1 { get; set; }

        [DatabaseModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DatabaseModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }

    public class PublisherModel : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [DatabaseModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DatabaseModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DatabaseModelProperty]
        public PublisherType Type { get; set; }

        [DatabaseModelProperty]
        public int? Number { get; set; }

        [DatabaseModelProperty]
        public int? Number1 { get; set; } = 111;

        [DatabaseModelProperty]
        public PublisherType? Type2 { get; set; }

        [DatabaseModelProperty]
        public PublisherType? Type3 { get; set; }

        [DatabaseModelProperty]
        public string? Name2 { get; set; }

        [DatabaseModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DatabaseModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DatabaseModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }

    public class Guid_PublisherModel : GuidDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [DatabaseModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DatabaseModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DatabaseModelProperty]
        public PublisherType Type { get; set; }

        [DatabaseModelProperty]
        public int? Number { get; set; }

        [DatabaseModelProperty]
        public int? Number1 { get; set; } = 111;

        [DatabaseModelProperty]
        public PublisherType? Type2 { get; set; }

        [DatabaseModelProperty]
        public PublisherType? Type3 { get; set; }

        [DatabaseModelProperty]
        public string? Name2 { get; set; }

        [DatabaseModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DatabaseModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DatabaseModelProperty]
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

    public class PublisherModel3_Client : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public int Integer { get; set; } = 999;

        [DatabaseModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DatabaseModelProperty]
        public string Name { get; set; } = "Name";

        [DatabaseModelProperty]
        public string Name2 { get; set; } = "Name2";

        [DatabaseModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    public class PublisherModel2_Client : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public int Integer { get; set; } = 999;

        [DatabaseModelProperty]
        public float Float { get; set; } = 1.9877f;

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [DatabaseModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [DatabaseModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [DatabaseModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [DatabaseModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [DatabaseModelProperty]
        public int? Number { get; set; } = 12121221;

        [DatabaseModelProperty]
        public int? Number1 { get; set; }

        [DatabaseModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [DatabaseModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }

    public class PublisherModel_Client : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [DatabaseModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DatabaseModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DatabaseModelProperty]
        public PublisherType Type { get; set; }

        [DatabaseModelProperty]
        public int? Number { get; set; }

        [DatabaseModelProperty]
        public int? Number1 { get; set; } = 111;

        [DatabaseModelProperty]
        public PublisherType? Type2 { get; set; }

        [DatabaseModelProperty]
        public PublisherType? Type3 { get; set; }

        [DatabaseModelProperty]
        public string? Name2 { get; set; }

        [DatabaseModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DatabaseModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DatabaseModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }

    public class Guid_PublisherModel_Client : GuidDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [DatabaseModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [DatabaseModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [DatabaseModelProperty]
        public PublisherType Type { get; set; }

        [DatabaseModelProperty]
        public int? Number { get; set; }

        [DatabaseModelProperty]
        public int? Number1 { get; set; } = 111;

        [DatabaseModelProperty]
        public PublisherType? Type2 { get; set; }

        [DatabaseModelProperty]
        public PublisherType? Type3 { get; set; }

        [DatabaseModelProperty]
        public string? Name2 { get; set; }

        [DatabaseModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [DatabaseModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [DatabaseModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }
}