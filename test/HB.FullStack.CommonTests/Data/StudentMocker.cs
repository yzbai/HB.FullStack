using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace HB.FullStack.CommonTests
{
    public class StudentMocker
    {
        public static Student MockOneStudent()
        {
            Student student = new Student
            {
                ChineseName = @"国华&*&（*（（""",
                Age = 12,
                StudentType = StudentType.TypeA,
                Books = new List<Book> { new Book { Name = "aa", Price = 12.121 }, new Book { Name = "xx", Price = 21312.2332342 } }
            };

            return student;
        }
    }

    public class Student
    {
        public string ChineseName { get; set; } = default!;

        public StudentType StudentType { get; set; }

        public int Age { get; set; }

        public IList<Book> Books { get; set; } = new List<Book>();

    }

    //[Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum StudentType
    {
        TypeA,
        TypeB
    }

    public class Book
    {
        public string Name { get; set; } = default!;

        public double Price { get; set; }
    }
}
