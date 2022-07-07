using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.DatabaseModels;

namespace HB.FullStack.DatabaseTests
{

    public class A : FlackIdModel
    {


        [ModelProperty]
        public string Name { get; set; } = default!;
    }


    public class B : FlackIdModel
    {

        [ModelProperty]
        public string Name { get; set; } = default!;
    }


    public class AB : FlackIdModel
    {

        [ModelProperty]
        public long AId { get; set; } = default!;

        [ModelProperty]
        public long BId { get; set; } = default!;
    }


    public class C : FlackIdModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public long AId { get; set; } = default!;
    }

    public class A_Client : FlackIdModel
    {

        [ModelProperty]
        public string Name { get; set; } = default!;
    }


    public class B_Client : FlackIdModel
    {

        [ModelProperty]
        public string Name { get; set; } = default!;
    }


    public class AB_Client : FlackIdModel
    {

        [ModelProperty]
        public long AId { get; set; } = default!;

        [ModelProperty]
        public long BId { get; set; } = default!;
    }


    public class C_Client : FlackIdModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public long AId { get; set; } = default!;
    }

    public class Guid_A : GuidModel
    {


        [ModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B : GuidModel
    {

        [ModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB : GuidModel
    {

        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
    }


    public class Guid_C : GuidModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public Guid Guid_AId { get; set; }
    }

    public class Guid_A_Client : GuidModel
    {

        [ModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B_Client : GuidModel
    {

        [ModelProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB_Client : GuidModel
    {

        [ModelProperty]
        public Guid Guid_AId { get; set; } = default!;

        [ModelProperty]
        public Guid Guid_BId { get; set; } = default!;
    }


    public class Guid_C_Client : GuidModel
    {
        [ModelProperty]
        public string Name { get; set; } = default!;

        [ModelProperty]
        public Guid Guid_AId { get; set; } = default!;
    }
}
