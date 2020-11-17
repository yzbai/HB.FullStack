using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.DatabaseTests.Data
{
    public static class Mocker
    {
        private static readonly Random _random = new Random();

        public static IList<BookEntity> GetBooks()
        {
            List<BookEntity> books = new List<BookEntity>();

            for (int i = 0; i < 5; ++i)
            {
                books.Add(new BookEntity
                {
                    Guid = SecurityUtil.CreateUniqueToken(),
                    Name = "Book" + i.ToString(),
                    Price = _random.NextDouble()
                });
            }

            return books;
        }

        public static PublisherEntity MockOne()
        {
            PublisherEntity entity = new PublisherEntity
            {
                Type = PublisherType.Online,
                Name = "中文名字",
                Books = new List<string>() { "Cat", "Dog" },
                //BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                BookAuthors = new Dictionary<string, Author>()
                {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                }
            };

            return entity;
        }

        public static IList<PublisherEntity> GetPublishers()
        {
            List<PublisherEntity> publisherEntities = new List<PublisherEntity>();

            for (int i = 0; i < 5; ++i)
            {
                publisherEntities.Add(new PublisherEntity
                {
                    Books = new List<string> { "a", "v", "c" },
                    Type = PublisherType.Big,
                    Name = "Publisher" + i.ToString(),
                    BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                    BookAuthors = new Dictionary<string, Author> { { "a", new Author { Mobile = "xxxx", Name = "tttt" } }, { "xxx", new Author { Mobile = "gggg", Name = "safas" } } }
                });
            }

            return publisherEntities;
        }
    }
}
