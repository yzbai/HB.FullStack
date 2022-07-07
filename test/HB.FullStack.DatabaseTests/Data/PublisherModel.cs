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
    public class PublisherModel3 : FlackIdModel
    {
        [ModelProperty]
        public int Integer { get; set; } = 999;

        [ModelProperty]
        public float Float { get; set; } = 1.9877f;

        [ModelProperty]
        public string Name { get; set; } = "Name";

        [ModelProperty]
        public string Name2 { get; set; } = "Name2";

        [ModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    public class Guid_PublisherModel3 : GuidModel
    {
        [ModelProperty]
        public int Integer { get; set; } = 999;

        [ModelProperty]
        public float Float { get; set; } = 1.9877f;

        [ModelProperty]
        public string Name { get; set; } = "Name";

        [ModelProperty]
        public string Name2 { get; set; } = "Name2";

        [ModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    public class PublisherModel2 : FlackIdModel
    {
        [ModelProperty]
        public int Integer { get; set; } = 999;

        [ModelProperty]
        public float Float { get; set; } = 1.9877f;

        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [ModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [ModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [ModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [ModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [ModelProperty]
        public int? Number { get; set; } = 12121221;

        [ModelProperty]
        public int? Number1 { get; set; }

        [ModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [ModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }

    public class Guid_PublisherModel2 : GuidModel
    {
        [ModelProperty]
        public int Integer { get; set; } = 999;

        [ModelProperty]
        public float Float { get; set; } = 1.9877f;

        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [ModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [ModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [ModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [ModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [ModelProperty]
        public int? Number { get; set; } = 12121221;

        [ModelProperty]
        public int? Number1 { get; set; }

        [ModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [ModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }

    public class PublisherModel : FlackIdModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [ModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [ModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [ModelProperty]
        public PublisherType Type { get; set; }

        [ModelProperty]
        public int? Number { get; set; }

        [ModelProperty]
        public int? Number1 { get; set; } = 111;

        [ModelProperty]
        public PublisherType? Type2 { get; set; }

        [ModelProperty]
        public PublisherType? Type3 { get; set; }

        [ModelProperty]
        public string? Name2 { get; set; }

        [ModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [ModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [ModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }

    public class Guid_PublisherModel : GuidModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [ModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [ModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [ModelProperty]
        public PublisherType Type { get; set; }

        [ModelProperty]
        public int? Number { get; set; }

        [ModelProperty]
        public int? Number1 { get; set; } = 111;

        [ModelProperty]
        public PublisherType? Type2 { get; set; }

        [ModelProperty]
        public PublisherType? Type3 { get; set; }

        [ModelProperty]
        public string? Name2 { get; set; }

        [ModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [ModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [ModelProperty]
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

    public class PublisherModel3_Client : FlackIdModel
    {
        [ModelProperty]
        public int Integer { get; set; } = 999;

        [ModelProperty]
        public float Float { get; set; } = 1.9877f;

        [ModelProperty]
        public string Name { get; set; } = "Name";

        [ModelProperty]
        public string Name2 { get; set; } = "Name2";

        [ModelProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    public class PublisherModel2_Client : FlackIdModel
    {
        [ModelProperty]
        public int Integer { get; set; } = 999;

        [ModelProperty]
        public float Float { get; set; } = 1.9877f;

        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public string? Name2 { get; set; } = "Name2";

        [ModelProperty]
        public string? Name3 { get; set; } //= "xxxx";

        [ModelProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [ModelProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [ModelProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [ModelProperty]
        public int? Number { get; set; } = 12121221;

        [ModelProperty]
        public int? Number1 { get; set; }

        [ModelProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);

        [ModelProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);
    }

    public class PublisherModel_Client : FlackIdModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [ModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [ModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [ModelProperty]
        public PublisherType Type { get; set; }

        [ModelProperty]
        public int? Number { get; set; }

        [ModelProperty]
        public int? Number1 { get; set; } = 111;

        [ModelProperty]
        public PublisherType? Type2 { get; set; }

        [ModelProperty]
        public PublisherType? Type3 { get; set; }

        [ModelProperty]
        public string? Name2 { get; set; }

        [ModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [ModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [ModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }

    public class Guid_PublisherModel_Client : GuidModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [ModelProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [ModelProperty(MaxLength = DefaultLengthConventions.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [ModelProperty]
        public PublisherType Type { get; set; }

        [ModelProperty]
        public int? Number { get; set; }

        [ModelProperty]
        public int? Number1 { get; set; } = 111;

        [ModelProperty]
        public PublisherType? Type2 { get; set; }

        [ModelProperty]
        public PublisherType? Type3 { get; set; }

        [ModelProperty]
        public string? Name2 { get; set; }

        [ModelProperty]
        public string? Name3 { get; set; } = "xxxx";

        [ModelProperty]
        public DateTimeOffset? DDD { get; set; }

        [ModelProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;
    }
}