using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Database.Test
{
    public class TestEntity : DatabaseEntity
    {
        [DatabaseEntityProperty]
        public string Name { get; set; }

        [DatabaseEntityProperty]
        public IList<string> Books { get; set; }

        [DatabaseEntityProperty]
        public IDictionary<string, string> BookAuthors { get; set; }

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
}
