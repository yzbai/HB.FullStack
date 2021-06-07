using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playground
{
    public record Person(string name, int age)
    {
        public IList<string> Addresses { get; init; }
    }

    public class Program
    {
        static void Main()
        {
            Person p = new Person("11", 11) { Addresses = new List<string>() };


            p.Addresses.Add("xx");

            typeof(Person).GetProperty("name").SetValue(p, "sfs");

            Console.WriteLine(p);

        }
    }
}
