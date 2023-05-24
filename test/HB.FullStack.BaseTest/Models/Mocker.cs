using System;
using System.Collections.Generic;

using HB.FullStack.BaseTest.Models;
using HB.FullStack.Common.IdGen;

namespace HB.FullStack.CacheTests
{
    public static class Mocker
    {
        private static readonly Random _random = new Random();

        public static Book MockOne()
        {
            return new Book
            {
                Id = StaticIdGen.GetLongId(),
                Name = SecurityUtil.CreateUniqueToken(),
                BookID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Publisher = _random.Next().ToString(),
                Price = _random.NextDouble() * 1000
            };
        }

        public static List<Book> MockMany(int count = 100)
        {
            List<Book> books = new List<Book>();

            for (int i = 0; i < count; ++i)
            {
                books.Add(new Book
                {
                    Id = StaticIdGen.GetLongId(),
                    Name = SecurityUtil.CreateUniqueToken(),
                    BookID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _random.Next(10000, 19999),
                    Publisher = _random.Next().ToString(),
                    Price = _random.NextDouble() * 1000
                });
            }

            return books;
        }



        public static IList<BookModel_Client> GetBooks_Client(int? count = null)
        {
            List<BookModel_Client> books = new List<BookModel_Client>();

            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                books.Add(new BookModel_Client
                {
                    Id = StaticIdGen.GetLongId(),
                    //Guid = SecurityUtil.CreateUniqueToken(),
                    Name = "Book" + i.ToString(),
                    Price = _random.NextDouble()
                });
            }

            return books;
        }

        public static IList<string> MockResourcesWithOne()
        {
            return new List<string> { "aa" };
        }

        public static IList<string> MockResourcesWithThree()
        {
            return new List<string> { "aa", "bb", "cc" };
        }
    }
}