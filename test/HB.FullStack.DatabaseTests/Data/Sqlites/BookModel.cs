using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests.Data.Sqlites
{
    public enum PublisherType
    {
        Online,
        Big,
        Small
    }

    public class Author
    {
        public string Name { get; set; } = default!;

        public string Mobile { get; set; } = default!;
    }

    [DbModel(DbSchema_Sqlite)]
    public class BookModel : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Sqlite)]
    public class Guid_BookModel : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Sqlite)]
    public class Book : TimestampFlackIdDbModel
    {
        [DbModelProperty]

        public string Name { get; set; } = null!;

        [DbModelProperty]

        public long BookID { get; set; }

        [DbModelProperty]
        public string? Publisher { get; set; }

        [DbModelProperty]
        public double Price { get; set; }
    }

    [DbModel(DbSchema_Sqlite)]
    public class BookModel_Client : TimestampFlackIdDbModel
    {

        [DbModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Sqlite)]
    public class Book_Client : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = null!;

        [DbModelProperty]

        public long BookID { get; set; }

        [DbModelProperty]
        public string? Publisher { get; set; }

        [DbModelProperty]
        public double Price { get; set; }
    }
}
