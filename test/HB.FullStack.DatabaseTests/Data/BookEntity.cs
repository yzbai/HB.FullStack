using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.Entities;

namespace HB.FullStack.DatabaseTests.Data
{
    
    public class BookEntity : FlackIdEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    
    public class Guid_BookEntity : GuidEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    
    public class Book : FlackIdEntity
    {
        [EntityProperty]
        
        public string Name { get; set; } = null!;

        [EntityProperty]
        
        public long BookID { get; set; }

        [EntityProperty]
        public string? Publisher { get; set; }

        [EntityProperty]
        public double Price { get; set; }
    }

    
    public class BookEntity_Client : FlackIdEntity
    {

        [EntityProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public double Price { get; set; } = default!;
    }

    
    public class Book_Client : FlackIdEntity
    {
        
        [EntityProperty]
        public string Name { get; set; } = null!;

        [EntityProperty]
        
        public long BookID { get; set; }

        [EntityProperty]
        public string? Publisher { get; set; }

        [EntityProperty]
        public double Price { get; set; }
    }
}
