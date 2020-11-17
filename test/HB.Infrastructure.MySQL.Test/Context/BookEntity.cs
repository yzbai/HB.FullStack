using HB.Framework.Common.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.DatabaseTests.Data
{
    [DatabaseEntity]
    public class BookEntity : Entity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }
}
