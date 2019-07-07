using HB.Framework.Database.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DatabaseSample
{
    [EntitySchema("test_db")]
    class BookEntity : DatabaseEntity
    {
        [UniqueGuidEntityProperty]
        public string Guid { get; set; }

        [EntityProperty]
        public string Name { get; set; }

        [EntityProperty]
        public double Price { get; set; }

        [EntityPropertyIgnore]
        public string Nonsence { get; set; }
    }
}
