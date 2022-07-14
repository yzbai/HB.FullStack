using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.DatabaseTests
{

    public class A : TimestampFlackIdDBModel
    {


        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class B : TimestampFlackIdDBModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class AB : TimestampFlackIdDBModel
    {

        [DatabaseModelProperty]
        public long AId { get; set; } = default!;

        [DatabaseModelProperty]
        public long BId { get; set; } = default!;
    }


    public class C : TimestampFlackIdDBModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public long AId { get; set; } = default!;
    }

    public class A_Client : TimestampFlackIdDBModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class B_Client : TimestampFlackIdDBModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class AB_Client : TimestampFlackIdDBModel
    {

        [DatabaseModelProperty]
        public long AId { get; set; } = default!;

        [DatabaseModelProperty]
        public long BId { get; set; } = default!;
    }


    public class C_Client : TimestampFlackIdDBModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public long AId { get; set; } = default!;
    }

    public class Guid_A : TimestampGuidDBModel
    {


        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B : TimestampGuidDBModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB : TimestampGuidDBModel
    {

        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
    }


    public class Guid_C : TimestampGuidDBModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public Guid Guid_AId { get; set; }
    }

    public class Guid_A_Client : TimestampGuidDBModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B_Client : TimestampGuidDBModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB_Client : TimestampGuidDBModel
    {

        [DatabaseModelProperty]
        public Guid Guid_AId { get; set; } = default!;

        [DatabaseModelProperty]
        public Guid Guid_BId { get; set; } = default!;
    }


    public class Guid_C_Client : TimestampGuidDBModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public Guid Guid_AId { get; set; } = default!;
    }
}
