using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.DatabaseTests.Data
{
    
    public class BookModel : FlackIdModel
    {

        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public double Price { get; set; } = default!;
    }

    
    public class Guid_BookModel : GuidModel
    {

        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public double Price { get; set; } = default!;
    }

    
    public class Book : FlackIdModel
    {
        [ModelProperty]
        
        public string Name { get; set; } = null!;

        [ModelProperty]
        
        public long BookID { get; set; }

        [ModelProperty]
        public string? Publisher { get; set; }

        [ModelProperty]
        public double Price { get; set; }
    }

    
    public class BookModel_Client : FlackIdModel
    {

        [ModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public double Price { get; set; } = default!;
    }

    
    public class Book_Client : FlackIdModel
    {
        
        [ModelProperty]
        public string Name { get; set; } = null!;

        [ModelProperty]
        
        public long BookID { get; set; }

        [ModelProperty]
        public string? Publisher { get; set; }

        [ModelProperty]
        public double Price { get; set; }
    }
}
