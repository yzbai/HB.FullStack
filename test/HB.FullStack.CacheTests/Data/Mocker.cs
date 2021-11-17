using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.CacheTests
{
    public static class Mocker
    {
        private static readonly Random _random = new Random();

        public static Book MockOne()
        {
            return new Book
            {
                Name = SecurityUtil.CreateUniqueToken(),
                BookID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Publisher = _random.Next().ToString(),
                Price = _random.NextDouble() * 1000
            };
        }

        public static List<Book> MockMany()
        {
            List<Book> books = new List<Book>();

            for (int i = 0; i < 100; ++i)
            {
                books.Add(new Book
                {
                    //Guid = "Guid" + i.ToString(),
                    Name = SecurityUtil.CreateUniqueToken(),
                    BookID = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _random.Next(10000, 19999),
                    Publisher = _random.Next().ToString(),
                    Price = _random.NextDouble() * 1000
                });
            }

            return books;
        }

        public static IList<BookEntity> GetBooks(int? count = null)
        {
            List<BookEntity> books = new List<BookEntity>();

            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                books.Add(new BookEntity
                {
                    //Guid = SecurityUtil.CreateUniqueToken(),
                    Name = "Book" + i.ToString(),
                    Price = _random.NextDouble()
                });
            }

            return books;
        }

        public static IList<Guid_BookEntity> Guid_GetBooks(int? count = null)
        {
            List<Guid_BookEntity> books = new List<Guid_BookEntity>();

            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                books.Add(new Guid_BookEntity
                {
                    //Guid = SecurityUtil.CreateUniqueToken(),
                    Name = "Book" + i.ToString(),
                    Price = _random.NextDouble()
                });
            }

            return books;
        }

        public static IList<BookEntity_Client> GetBooks_Client(int? count = null)
        {
            List<BookEntity_Client> books = new List<BookEntity_Client>();

            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                books.Add(new BookEntity_Client
                {
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