using System;
using HB.FullStack.Common.Models;

namespace HB.FullStack.CommonTests.Data
{

    public class BookRes : SharedResource
    {
        public string? Name { get; set; }

        public string? Title { get; set; }

        public double Price { get; set; }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Title, Price);
        }
    }
}