using HB.Framework.Common;
using HB.Framework.Common.Entities;
using HB.Framework.Database.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace XamarinSample.Models
{
    [DatabaseEntity]
    public class PublisherEntity : Entity
    {

        [EntityProperty]
        public string Name { get; set; }

        [EntityProperty]
        public IList<string> Books { get; set; }

        [EntityProperty(Converter = typeof(PublisherBookAuthorsTypeConventer))]
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
            return SerializeUtil.FromJson<IDictionary<string, Author>>(stringValue);
        }

        protected override string TypeValueToStringDbValue(object typeValue)
        {
            return SerializeUtil.ToJson(typeValue);
        }
    }
}
