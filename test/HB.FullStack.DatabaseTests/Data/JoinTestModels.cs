using System;

using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests
{

    public class A : TimestampFlackIdDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }

    public class B : TimestampFlackIdDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }

    public class AB : TimestampFlackIdDbModel
    {

        [DBModelProperty]
        public long AId { get; set; } = default!;

        [DBModelProperty]
        public long BId { get; set; } = default!;
    }

    public class C : TimestampFlackIdDbModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public long AId { get; set; } = default!;
    }

    public class A_Client : TimestampFlackIdDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }

    public class B_Client : TimestampFlackIdDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }

    public class AB_Client : TimestampFlackIdDbModel
    {

        [DBModelProperty]
        public long AId { get; set; } = default!;

        [DBModelProperty]
        public long BId { get; set; } = default!;
    }

    public class C_Client : TimestampFlackIdDbModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public long AId { get; set; } = default!;
    }

    public class Guid_A : TimestampGuidDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }

    public class Guid_B : TimestampGuidDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }

    public class Guid_AB : TimestampGuidDbModel
    {

        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
    }

    public class Guid_C : TimestampGuidDbModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public Guid Guid_AId { get; set; }
    }

    public class Guid_A_Client : TimestampGuidDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }

    public class Guid_B_Client : TimestampGuidDbModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }

    public class Guid_AB_Client : TimestampGuidDbModel
    {

        [DBModelProperty]
        public Guid Guid_AId { get; set; } = default!;

        [DBModelProperty]
        public Guid Guid_BId { get; set; } = default!;
    }

    public class Guid_C_Client : TimestampGuidDbModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public Guid Guid_AId { get; set; } = default!;
    }
}
