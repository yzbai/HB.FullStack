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

        [EntityProperty]
        public IList<string> Books { get; set; } = default!;

        [EntityProperty(Converter = typeof(PublisherBookAuthorsTypeConventer))]
        public IDictionary<string, Author> BookAuthors { get; set; } = default!;

        [EntityProperty(Length = EntityPropertyLength.MediumLength)]
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
}
