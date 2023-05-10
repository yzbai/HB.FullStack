using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.Sqlites
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
    [CacheModel]
    public class Book2Model : TimestampFlackIdDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Sqlite)]
    [CacheModel]
    public class Guid_BookModel : TimestampGuidDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Sqlite)]
    [CacheModel]
    public class Book : TimestampFlackIdDbModel
    {
        [DbField]
        [CacheModelAltKey]
        public string Name { get; set; } = null!;

        [DbField]
        [CacheModelAltKey]
        public long BookID { get; set; }

        [DbField]
        public string? Publisher { get; set; }

        [DbField]
        public double Price { get; set; }
    }

    [DbModel(DbSchema_Sqlite)]
    [CacheModel]
    public class BookModel_Client : TimestampFlackIdDbModel
    {

        [DbField(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Sqlite)]
    [CacheModel]
    public class Book_Client : TimestampFlackIdDbModel
    {

        [DbField]
        [CacheModelAltKey]
        public string Name { get; set; } = null!;

        [DbField]
        [CacheModelAltKey]
        public long BookID { get; set; }

        [DbField]
        public string? Publisher { get; set; }

        [DbField]
        public double Price { get; set; }
    }
}
