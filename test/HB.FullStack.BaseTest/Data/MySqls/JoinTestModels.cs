using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.MySqls
{

    [DbTable(DbSchema_Mysql)]
    public class A : TimestampFlackIdDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;
    }

    [DbTable(DbSchema_Mysql)]
    public class B : TimestampFlackIdDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;
    }
    [DbTable(DbSchema_Mysql)]
    public class AB : TimestampFlackIdDbModel
    {

        [DbField]
        public long AId { get; set; } = default!;

        [DbField]
        public long BId { get; set; } = default!;
    }
    [DbTable(DbSchema_Mysql)]
    public class C : TimestampFlackIdDbModel
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public long AId { get; set; } = default!;
    }

    [DbTable(DbSchema_Mysql)]
    public class Guid_A : TimestampGuidDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;
    }
    [DbTable(DbSchema_Mysql)]
    public class Guid_B : TimestampGuidDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;
    }
    [DbTable(DbSchema_Mysql)]
    public class Guid_AB : TimestampGuidDbModel
    {

        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
    }
    [DbTable(DbSchema_Mysql)]
    public class Guid_C : TimestampGuidDbModel
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public Guid Guid_AId { get; set; }
    }

}
