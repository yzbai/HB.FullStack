using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests.Data.MySqls
{
    [DbModel(DbSchema_Mysql)]
    public class Book2Model : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Mysql)]
    public class Guid_BookModel : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Mysql)]
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

    [DbModel(DbSchema_Mysql)]
    public class BookModel_Client : TimestampFlackIdDbModel
    {

        [DbModelProperty(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Mysql)]
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
