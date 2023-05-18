using HB.FullStack.Common;
using HB.FullStack.Common.IdGen;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{
    [DbModel(DbSchema_Mysql)]
    public class Book2Model : DbModel2<long>, ITimestamp
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
        public long Timestamp { get; set; }

        public override long Id { get; set; } = StaticIdGen.GetLongId();

        public override bool Deleted { get; set; }
        public override string LastUser { get; set; } = null!;
    }

    [DbModel(DbSchema_Mysql)]
    public class Guid_BookModel : DbModel2<Guid>, ITimestamp
    {

        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
        public long Timestamp { get; set; }
        public override Guid Id { get; set; } = StaticIdGen.GetSequentialGuid();
        public override bool Deleted { get; set; }
        public override string LastUser { get; set; } = null!;
    }

    [DbModel(DbSchema_Mysql)]
    public class Guid_BookModel_Timeless : DbModel2<Guid>
    {

        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
        public override Guid Id { get; set; } = StaticIdGen.GetSequentialGuid();
        public override bool Deleted { get; set; }
        public override string LastUser { get; set; } = null!;
    }

    [DbModel(DbSchema_Mysql)]
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

    [DbModel(DbSchema_Mysql)]
    public class BookModel_Client : TimestampFlackIdDbModel
    {

        [DbField(NeedIndex = true)]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;
    }

    [DbModel(DbSchema_Mysql)]
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
