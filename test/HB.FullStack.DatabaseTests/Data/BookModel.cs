using HB.FullStack.Database.DBModels;

namespace HB.FullStack.DatabaseTests.Data
{

    public class BookModel : TimestampFlackIdDBModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }


    public class Guid_BookModel : TimestampGuidDBModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }


    public class Book : TimestampFlackIdDBModel
    {
        [DBModelProperty]

        public string Name { get; set; } = null!;

        [DBModelProperty]

        public long BookID { get; set; }

        [DBModelProperty]
        public string? Publisher { get; set; }

        [DBModelProperty]
        public double Price { get; set; }
    }


    public class BookModel_Client : TimestampFlackIdDBModel
    {

        [DBModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }


    public class Book_Client : TimestampFlackIdDBModel
    {

        [DBModelProperty]
        public string Name { get; set; } = null!;

        [DBModelProperty]

        public long BookID { get; set; }

        [DBModelProperty]
        public string? Publisher { get; set; }

        [DBModelProperty]
        public double Price { get; set; }
    }
}
