using HB.FullStack.Common;
using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Entities;

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.FullStack.DatabaseTests.Data
{

    [DatabaseEntity]
    public class PublisherEntity3 : Entity
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

    [DatabaseEntity]
    public class PublisherEntity2 : Entity
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
        public int? Number1 { get; set; } = 111;


        [EntityProperty]
        public DateTimeOffset? DDD { get; set; }// = new DateTimeOffset(2021, 11, 11, 11, 11, 11, TimeSpan.Zero);


        [EntityProperty]
        public DateTimeOffset? EEE { get; set; } = new DateTimeOffset(2020, 12, 12, 12, 12, 12, TimeSpan.Zero);


    }

    [DatabaseEntity]
    public class PublisherEntity : Entity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty(Converter = typeof(PublisherBooksTypeConventer))]
        public IList<string> Books { get; set; } = default!;

        [EntityProperty(Converter = typeof(PublisherBookAuthorsTypeConventer))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [EntityProperty(Length = EntityPropertyLength.MediumLength, Converter = typeof(PublisherBookNamesTypeConventer))]
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

    public class PublisherBookAuthorsTypeConventer : DatabaseTypeConverter
    {
        public PublisherBookAuthorsTypeConventer() { }

        protected override object? StringDbValueToTypeValue(string stringValue)
        {
            return SerializeUtil.FromJson<IDictionary<string, Author>>(stringValue);
        }

        protected override string TypeValueToStringDbValue(object typeValue)
        {
            return SerializeUtil.ToJson(typeValue);
        }
    }

    public class PublisherBookNamesTypeConventer : DatabaseTypeConverter
    {
        public PublisherBookNamesTypeConventer() { }

        protected override object? StringDbValueToTypeValue(string stringValue)
        {
            return SerializeUtil.FromJson<IDictionary<string, string>>(stringValue);
        }

        protected override string TypeValueToStringDbValue(object typeValue)
        {
            return SerializeUtil.ToJson(typeValue);
        }
    }

    public class PublisherBooksTypeConventer : DatabaseTypeConverter
    {
        public PublisherBooksTypeConventer() { }

        protected override object? StringDbValueToTypeValue(string stringValue)
        {
            return SerializeUtil.FromJson<IList<string>>(stringValue);
        }

        protected override string TypeValueToStringDbValue(object typeValue)
        {
            return SerializeUtil.ToJson(typeValue);
        }
    }
}
