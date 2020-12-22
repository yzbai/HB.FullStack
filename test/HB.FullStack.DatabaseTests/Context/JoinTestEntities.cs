using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;

namespace HB.FullStack.DatabaseTests
{

    public class A : DatabaseEntity
    {


        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class B : DatabaseEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class AB : DatabaseEntity
    {

        [EntityProperty]
        public long AId { get; set; } = default!;

        [EntityProperty]
        public long BId { get; set; } = default!;
    }


    public class C : DatabaseEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public long AId { get; set; } = default!;
    }
}
