﻿using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.Entities;

namespace HB.FullStack.DatabaseTests
{

    public class A : FlackIdEntity
    {


        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class B : FlackIdEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class AB : FlackIdEntity
    {

        [EntityProperty]
        public long AId { get; set; } = default!;

        [EntityProperty]
        public long BId { get; set; } = default!;
    }


    public class C : FlackIdEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public long AId { get; set; } = default!;
    }

    public class A_Client : FlackIdEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class B_Client : FlackIdEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class AB_Client : FlackIdEntity
    {

        [EntityProperty]
        public long AId { get; set; } = default!;

        [EntityProperty]
        public long BId { get; set; } = default!;
    }


    public class C_Client : FlackIdEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public long AId { get; set; } = default!;
    }

    public class Guid_A : GuidEntity
    {


        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B : GuidEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB : GuidEntity
    {

        public Guid Guid_AId { get; set; }

        public Guid Guid_BId { get; set; }
    }


    public class Guid_C : GuidEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public Guid Guid_AId { get; set; }
    }

    public class Guid_A_Client : GuidEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_B_Client : GuidEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class Guid_AB_Client : GuidEntity
    {

        [EntityProperty]
        public Guid Guid_AId { get; set; } = default!;

        [EntityProperty]
        public Guid Guid_BId { get; set; } = default!;
    }


    public class Guid_C_Client : GuidEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public Guid Guid_AId { get; set; } = default!;
    }
}
