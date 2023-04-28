using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.Sqlites
{


    [DbTable(DbSchema_Sqlite)]
    public class A_Client : TimestampFlackIdDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;
    }
    [DbTable(DbSchema_Sqlite)]
    public class B_Client : TimestampFlackIdDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;
    }
    [DbTable(DbSchema_Sqlite)]
    public class AB_Client : TimestampFlackIdDbModel
    {

        [DbField]
        public long AId { get; set; } = default!;

        [DbField]
        public long BId { get; set; } = default!;
    }
    [DbTable(DbSchema_Sqlite)]
    public class C_Client : TimestampFlackIdDbModel
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public long AId { get; set; } = default!;
    }

    [DbTable(DbSchema_Sqlite)]
    public class Guid_A_Client : TimestampGuidDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;
    }
    [DbTable(DbSchema_Sqlite)]
    public class Guid_B_Client : TimestampGuidDbModel
    {

        [DbField]
        public string Name { get; set; } = default!;
    }
    [DbTable(DbSchema_Sqlite)]
    public class Guid_AB_Client : TimestampGuidDbModel
    {

        [DbField]
        public Guid Guid_AId { get; set; } = default!;

        [DbField]
        public Guid Guid_BId { get; set; } = default!;
    }
    [DbTable(DbSchema_Sqlite)]
    public class Guid_C_Client : TimestampGuidDbModel
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public Guid Guid_AId { get; set; } = default!;
    }
}
