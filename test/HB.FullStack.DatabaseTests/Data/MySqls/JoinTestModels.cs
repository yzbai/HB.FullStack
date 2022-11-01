using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests.Data.MySqls
{

    [DbModel(DbSchema_Mysql)]
    public class A : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }

    [DbModel(DbSchema_Mysql)]
    public class B : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }
    [DbModel(DbSchema_Mysql)]
    public class AB : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public long AId { get; set; } = default!;

        [DbModelProperty]
        public long BId { get; set; } = default!;
    }
    [DbModel(DbSchema_Mysql)]
    public class C : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public long AId { get; set; } = default!;
    }

    [DbModel(DbSchema_Mysql)]
    public class Guid_A : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }
    [DbModel(DbSchema_Mysql)]
    public class Guid_B : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }
    [DbModel(DbSchema_Mysql)]
    public class Guid_AB : TimestampGuidDbModel
    {

        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
    }
    [DbModel(DbSchema_Mysql)]
    public class Guid_C : TimestampGuidDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public Guid Guid_AId { get; set; }
    }

}
