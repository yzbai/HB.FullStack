using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests.Data
{

    public class BookModel : TimestampFlackIdDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }

    public class Guid_BookModel : TimestampGuidDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }

    public class Book : TimestampFlackIdDbModel
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

    public class BookModel_Client : TimestampFlackIdDbModel
    {

        [DBModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public double Price { get; set; } = default!;
    }

    public class Book_Client : TimestampFlackIdDbModel
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
