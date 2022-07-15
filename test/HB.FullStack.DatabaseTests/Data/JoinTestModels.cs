using System;

using HB.FullStack.Database.DBModels;

namespace HB.FullStack.DatabaseTests
{

    public class A : TimestampFlackIdDBModel
    {


        [DBModelProperty]
        public string Name { get; set; } = default!;
    }


    public class B : TimestampFlackIdDBModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }


    public class AB : TimestampFlackIdDBModel
    {

        [DBModelProperty]
        public long AId { get; set; } = default!;

        [DBModelProperty]
        public long BId { get; set; } = default!;
    }


    public class C : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public long AId { get; set; } = default!;
    }

    public class A_Client : TimestampFlackIdDBModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }


    public class B_Client : TimestampFlackIdDBModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }


    public class AB_Client : TimestampFlackIdDBModel
    {

        [DBModelProperty]
        public long AId { get; set; } = default!;

        [DBModelProperty]
        public long BId { get; set; } = default!;
    }


    public class C_Client : TimestampFlackIdDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public long AId { get; set; } = default!;
    }

    public class Guid_A : TimestampGuidDBModel
    {


        [DBModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B : TimestampGuidDBModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB : TimestampGuidDBModel
    {

        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
    }


    public class Guid_C : TimestampGuidDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public Guid Guid_AId { get; set; }
    }

    public class Guid_A_Client : TimestampGuidDBModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B_Client : TimestampGuidDBModel
    {

        [DBModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB_Client : TimestampGuidDBModel
    {

        [DBModelProperty]
        public Guid Guid_AId { get; set; } = default!;

        [DBModelProperty]
        public Guid Guid_BId { get; set; } = default!;
    }


    public class Guid_C_Client : TimestampGuidDBModel
    {
        [DBModelProperty]
        public string Name { get; set; } = default!;

        [DBModelProperty]
        public Guid Guid_AId { get; set; } = default!;
    }
}
