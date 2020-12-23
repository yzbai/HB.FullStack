using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Entities;
using HB.FullStack.Database.Def;

namespace HB.FullStack.DatabaseTests
{

    public class A : AutoIcrementIdEntity
    {


        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class B : AutoIcrementIdEntity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }


    public class AB : AutoIcrementIdEntity
    {

        [EntityProperty]
        public long AId { get; set; } = default!;

        [EntityProperty]
        public long BId { get; set; } = default!;
    }


    public class C : AutoIcrementIdEntity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public long AId { get; set; } = default!;
    }
}
