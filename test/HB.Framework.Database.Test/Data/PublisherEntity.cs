using HB.Framework.Common;
using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.Framework.Database.Test
{
    public class PublisherEntity : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; }

        [EntityProperty]
        public string Name { get; set; }

        [EntityProperty]
        public IList<string> Books { get; set; }

        [EntityProperty(ConverterType = typeof(PublisherBookAuthorsTypeConventer))]
        public IDictionary<string, Author> BookAuthors { get; set; }

        [EntityProperty(Length = EntityPropertyLength.MediumLength)]
        public IDictionary<string, string> BookNames { get; set; }

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
        public string Name { get; set; }

        public string Mobile { get; set; }
    }

    public class PublisherBookAuthorsTypeConventer : DatabaseTypeConverter
    {
        public PublisherBookAuthorsTypeConventer() { }

        protected override object StringDbValueToTypeValue(string stringValue)
        {
            return JsonUtil.FromJson<IDictionary<string, Author>>(stringValue);
        }

        protected override string TypeValueToStringDbValue(object typeValue)
        {
            return JsonUtil.ToJson(typeValue);
        }
    }
}
