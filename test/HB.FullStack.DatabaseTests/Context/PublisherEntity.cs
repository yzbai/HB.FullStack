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
