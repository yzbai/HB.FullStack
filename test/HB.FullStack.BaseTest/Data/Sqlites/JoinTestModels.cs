using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.BaseTest.Data.Sqlites
{


    [DbModel(DbSchema_Sqlite)]
    public class A_Client : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }
    [DbModel(DbSchema_Sqlite)]
    public class B_Client : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }
    [DbModel(DbSchema_Sqlite)]
    public class AB_Client : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public long AId { get; set; } = default!;

        [DbModelProperty]
        public long BId { get; set; } = default!;
    }
    [DbModel(DbSchema_Sqlite)]
    public class C_Client : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public long AId { get; set; } = default!;
    }

    [DbModel(DbSchema_Sqlite)]
    public class Guid_A_Client : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }
    [DbModel(DbSchema_Sqlite)]
    public class Guid_B_Client : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }
    [DbModel(DbSchema_Sqlite)]
    public class Guid_AB_Client : TimestampGuidDbModel
    {

        [DbModelProperty]
        public Guid Guid_AId { get; set; } = default!;

        [DbModelProperty]
        public Guid Guid_BId { get; set; } = default!;
    }
    [DbModel(DbSchema_Sqlite)]
    public class Guid_C_Client : TimestampGuidDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public Guid Guid_AId { get; set; } = default!;
    }
}
