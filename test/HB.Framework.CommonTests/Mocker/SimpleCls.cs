using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.CommonTests.Mocker
{
    class SimpleCls
    {
        public string Name { get; set; }

        public string Mark { get; set; }

        public int Age { get; set; }

        public SimpleCls(string name, string mark, int age)
        {
            Name = name;
            Mark = mark;
            Age = age;
        }
    }
}
