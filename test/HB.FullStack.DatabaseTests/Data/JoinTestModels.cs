using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.DatabaseTests
{

    public class A : FlackIdDatabaseModel
    {


        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class B : FlackIdDatabaseModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class AB : FlackIdDatabaseModel
    {

        [DatabaseModelProperty]
        public long AId { get; set; } = default!;

        [DatabaseModelProperty]
        public long BId { get; set; } = default!;
    }


    public class C : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public long AId { get; set; } = default!;
    }

    public class A_Client : FlackIdDatabaseModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class B_Client : FlackIdDatabaseModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class AB_Client : FlackIdDatabaseModel
    {

        [DatabaseModelProperty]
        public long AId { get; set; } = default!;

        [DatabaseModelProperty]
        public long BId { get; set; } = default!;
    }


    public class C_Client : FlackIdDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public long AId { get; set; } = default!;
    }

    public class Guid_A : GuidDatabaseModel
    {


        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B : GuidDatabaseModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB : GuidDatabaseModel
    {

        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
    }


    public class Guid_C : GuidDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public Guid Guid_AId { get; set; }
    }

    public class Guid_A_Client : GuidDatabaseModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B_Client : GuidDatabaseModel
    {

        [DatabaseModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB_Client : GuidDatabaseModel
    {

        [DatabaseModelProperty]
        public Guid Guid_AId { get; set; } = default!;

        [DatabaseModelProperty]
        public Guid Guid_BId { get; set; } = default!;
    }


    public class Guid_C_Client : GuidDatabaseModel
    {
        [DatabaseModelProperty]
        public string Name { get; set; } = default!;

        [DatabaseModelProperty]
        public Guid Guid_AId { get; set; } = default!;
    }
}
