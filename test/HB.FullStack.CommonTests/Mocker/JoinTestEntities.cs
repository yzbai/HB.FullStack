using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Database.Entities;

namespace HB.FullStack.DatabaseTests
{

    public class A : IdGenEntity
    {


        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class B : IdGenEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class AB : IdGenEntity
    {

        [EntityProperty]
        public long AId { get; set; } = default!;

        [EntityProperty]
        public long BId { get; set; } = default!;
    }


    public class C : IdGenEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public long AId { get; set; } = default!;
    }

    public class A_Client : IdGenEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class B_Client : IdGenEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class AB_Client : IdGenEntity
    {

        [EntityProperty]
        public long AId { get; set; } = default!;

        [EntityProperty]
        public long BId { get; set; } = default!;
    }


    public class C_Client : IdGenEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public long AId { get; set; } = default!;
    }
}
