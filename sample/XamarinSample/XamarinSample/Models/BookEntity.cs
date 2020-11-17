using HB.Framework.Common.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace XamarinSample.Models
{
    [DatabaseEntity]
    public class BookEntity : Entity
    {
        [EntityProperty]
        public string Name { get; set; }

        [EntityProperty]
        public double Price { get; set; }
    }
}
