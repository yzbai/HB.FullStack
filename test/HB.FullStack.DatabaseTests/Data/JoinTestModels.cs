using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests
{

    public class A : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }

    public class B : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }

    public class AB : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public long AId { get; set; } = default!;

        [DbModelProperty]
        public long BId { get; set; } = default!;
    }

    public class C : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public long AId { get; set; } = default!;
    }

    public class A_Client : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }

    public class B_Client : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }

    public class AB_Client : TimestampFlackIdDbModel
    {

        [DbModelProperty]
        public long AId { get; set; } = default!;

        [DbModelProperty]
        public long BId { get; set; } = default!;
    }

    public class C_Client : TimestampFlackIdDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public long AId { get; set; } = default!;
    }

    public class Guid_A : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }

    public class Guid_B : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }

    public class Guid_AB : TimestampGuidDbModel
    {

        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
    }

    public class Guid_C : TimestampGuidDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public Guid Guid_AId { get; set; }
    }

    public class Guid_A_Client : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }

    public class Guid_B_Client : TimestampGuidDbModel
    {

        [DbModelProperty]
        public string Name { get; set; } = default!;
    }

    public class Guid_AB_Client : TimestampGuidDbModel
    {

        [DbModelProperty]
        public Guid Guid_AId { get; set; } = default!;

        [DbModelProperty]
        public Guid Guid_BId { get; set; } = default!;
    }

    public class Guid_C_Client : TimestampGuidDbModel
    {
        [DbModelProperty]
        public string Name { get; set; } = default!;

        [DbModelProperty]
        public Guid Guid_AId { get; set; } = default!;
    }
}
