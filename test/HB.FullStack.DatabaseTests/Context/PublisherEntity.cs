using HB.FullStack.Common;
using HB.FullStack.Common.Entities;
using HB.FullStack.Database;
using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Def;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.FullStack.DatabaseTests.Data
{

    [CacheEntity]
    public class PublisherEntity3 : IdGenEntity
    {
        [EntityProperty]
        public int Integer { get; set; } = 999;

        [EntityProperty]
        public float Float { get; set; } = 1.9877f;

        [EntityProperty]
        public string Name { get; set; } = "Name";


        [EntityProperty]
        public string Name2 { get; set; } = "Name2";

        [EntityProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    [CacheEntity]
    public class PublisherEntity2 : IdGenEntity
    {
        [EntityProperty]
        public int Integer { get; set; } = 999;

        [EntityProperty]
        public float Float { get; set; } = 1.9877f;

        [EntityProperty]
        public string Name { get; set; } = default!;


        [EntityProperty]
        public string? Name2 { get; set; } = "Name2";

        [EntityProperty]
        public string? Name3 { get; set; } //= "xxxx";


        [EntityProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [EntityProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [EntityProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [EntityProperty]
        public int? Number { get; set; } = 12121221;

        [EntityProperty]
        public int? Number1 { get; set; }


        [EntityProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);


        [EntityProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);


    }

    [CacheEntity]
    public class PublisherEntity : IdGenEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [EntityProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [EntityProperty(MaxLength = Consts.MediumLength, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [EntityProperty]
        public PublisherType Type { get; set; }

        [EntityProperty]
        public int? Number { get; set; }

        [EntityProperty]
        public int? Number1 { get; set; } = 111;

        [EntityProperty]
        public PublisherType? Type2 { get; set; }

        [EntityProperty]
        public PublisherType? Type3 { get; set; }

        [EntityProperty]
        public string? Name2 { get; set; }

        [EntityProperty]
        public string? Name3 { get; set; } = "xxxx";

        [EntityProperty]
        public DateTimeOffset? DDD { get; set; }


        [EntityProperty]
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

    [CacheEntity]
    public class PublisherEntity3_Client : IdGuidEntity
    {
        [EntityProperty]
        public int Integer { get; set; } = 999;

        [EntityProperty]
        public float Float { get; set; } = 1.9877f;

        [EntityProperty]
        public string Name { get; set; } = "Name";


        [EntityProperty]
        public string Name2 { get; set; } = "Name2";

        [EntityProperty]
        public string Name3 { get; set; } = "xxxx";
    }

    [CacheEntity]
    public class PublisherEntity2_Client : IdGuidEntity
    {
        [EntityProperty]
        public int Integer { get; set; } = 999;

        [EntityProperty]
        public float Float { get; set; } = 1.9877f;

        [EntityProperty]
        public string Name { get; set; } = default!;


        [EntityProperty]
        public string? Name2 { get; set; } = "Name2";

        [EntityProperty]
        public string? Name3 { get; set; } //= "xxxx";


        [EntityProperty]
        public PublisherType Type { get; set; } = PublisherType.Big;

        [EntityProperty]
        public PublisherType? Type2 { get; set; } = PublisherType.Small;

        [EntityProperty]
        public PublisherType? Type3 { get; set; } //= PublisherType.Online;

        [EntityProperty]
        public int? Number { get; set; } = 12121221;

        [EntityProperty]
        public int? Number1 { get; set; }


        [EntityProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);


        [EntityProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);


    }

    [CacheEntity]
    public class PublisherEntity_Client : IdGuidEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty(Converter = typeof(JsonTypeConverter))]
        public IList<string> Books { get; set; } = default!;

        [EntityProperty(Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [EntityProperty(MaxLength = Consts.MediumLength, Converter = typeof(JsonTypeConverter))]
        public IDictionary<string, string> BookNames { get; set; } = default!;

        [EntityProperty]
        public PublisherType Type { get; set; }

        [EntityProperty]
        public int? Number { get; set; }

        [EntityProperty]
        public int? Number1 { get; set; } = 111;

        [EntityProperty]
        public PublisherType? Type2 { get; set; }

        [EntityProperty]
        public PublisherType? Type3 { get; set; }

        [EntityProperty]
        public string? Name2 { get; set; }

        [EntityProperty]
        public string? Name3 { get; set; } = "xxxx";

        [EntityProperty]
        public DateTimeOffset? DDD { get; set; }


        [EntityProperty]
        public DateTimeOffset? EEE { get; set; } = DateTimeOffset.UtcNow;


    }
}
