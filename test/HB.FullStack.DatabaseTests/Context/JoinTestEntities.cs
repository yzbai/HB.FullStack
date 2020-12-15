using System;
using System.Collections.Generic;
using System.Text;

using HB.FullStack.Common.Entities;

namespace HB.FullStack.DatabaseTests
{
    [DatabaseEntity]
    public class A : Entity
    {


        [EntityProperty]
        public string Name { get; set; } = default!;
    }

    [DatabaseEntity]
    public class B : Entity
    {

        [EntityProperty]
        public string Name { get; set; } = default!;
    }

    [DatabaseEntity]
    public class AB : Entity
    {

        [EntityProperty]
        public string AId { get; set; } = default!;

        [EntityProperty]
        public string BId { get; set; } = default!;
    }

    [DatabaseEntity]
    public class C : Entity
    {
        [EntityProperty]
        public string Name { get; set; } = default!;

        [EntityProperty]
        public string AId { get; set; } = default!;
    }
}
