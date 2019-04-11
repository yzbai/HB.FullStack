using HB.Framework.Common;
using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HB.Framework.Database.Test
{
    public class TestEntity : DatabaseEntity
    {
        [DatabaseEntityProperty]
        public string Name { get; set; }

        [DatabaseEntityProperty]
        public IList<string> Books { get; set; }

        [DatabaseEntityProperty(ConverterType = typeof(TestEntityTypeConventer))]
        public IDictionary<string, Author> BookAuthors { get; set; }

        [DatabaseEntityProperty]
        public TestType Type { get; set; }
    }

    public enum TestType
    {
        Type1,
        Type2,
        Type3,
        Hahaha
    }

    public class Author
    {
        public string Name { get; set; }

        public string Mobile { get; set; }
    }

    public class TestEntityTypeConventer : DatabaseTypeConverter
    {
        public TestEntityTypeConventer() { }

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
