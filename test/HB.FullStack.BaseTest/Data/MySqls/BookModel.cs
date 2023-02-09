using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    [DbTable(DbSchema_Mysql)]
    public class Book2Model : TimestampFlackIdDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
    }

    [DbTable(DbSchema_Mysql)]
    public class Guid_BookModel : TimestampGuidDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
    }

    [DbTable(DbSchema_Mysql)]
    public class Book : TimestampFlackIdDbModel
    {
        [DbField]

        public string Name { get; set; } = null!;

        [DbField]

        public long BookID { get; set; }

        [DbField]
        public string? Publisher { get; set; }

        [DbField]
        public double Price { get; set; }
    }

    [DbTable(DbSchema_Mysql)]
    public class BookModel_Client : TimestampFlackIdDbModel
    {

        [DbField(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
    }

    [DbTable(DbSchema_Mysql)]
    public class Book_Client : TimestampFlackIdDbModel
    {

        [DbField]
        public string Name { get; set; } = null!;

        [DbField]

        public long BookID { get; set; }

        [DbField]
        public string? Publisher { get; set; }

        [DbField]
        public double Price { get; set; }
    }
}
