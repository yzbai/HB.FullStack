using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.DatabaseTests.Data
{
    
    public class BookModel : FlackIdDatabaseModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    
    public class Guid_BookModel : GuidDatabaseModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    
    public class Book : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        
        public string Name { get; set; } = null!;

        [DatabaseModelProperty]
        
        public long BookID { get; set; }

        [DatabaseModelProperty]
        public string? Publisher { get; set; }

        [DatabaseModelProperty]
        public double Price { get; set; }
    }

    
    public class BookModel_Client : FlackIdDatabaseModel
    {

        [DatabaseModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public double Price { get; set; } = default!;
    }

    
    public class Book_Client : FlackIdDatabaseModel
    {
        
        [DatabaseModelProperty]
        public string Name { get; set; } = null!;

        [DatabaseModelProperty]
        
        public long BookID { get; set; }

        [DatabaseModelProperty]
        public string? Publisher { get; set; }

        [DatabaseModelProperty]
        public double Price { get; set; }
    }
}
