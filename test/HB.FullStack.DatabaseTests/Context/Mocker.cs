using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.DatabaseTests.Data
{
    public static class Mocker
    {
        private static readonly Random _random = new Random();

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

        public static PublisherEntity MockOne()
        {
            PublisherEntity entity = new PublisherEntity
            {
                Type = PublisherType.Online,
                Name = "中文名字",
                Books = new List<string>() { "Cat", "Dog" },
                BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                BookAuthors = new Dictionary<string, Author>()
                {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                }
            };

            return entity;
        }

        public static IList<PublisherEntity> GetPublishers(int? count = null)
        {
            List<PublisherEntity> publisherEntities = new List<PublisherEntity>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherEntities.Add(new PublisherEntity
                {
                    Books = new List<string> { "a", "v", "c" },
                    Type = (PublisherType)random.Next(0, 3),
                    Name = "Publisher" + i.ToString(),
                    BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                    BookAuthors = new Dictionary<string, Author> { { "a", new Author { Mobile = "xxxx", Name = "tttt" } }, { "xxx", new Author { Mobile = "gggg", Name = "safas" } } }
                });
            }

            return publisherEntities;
        }

        public static IList<PublisherEntity2> GetPublishers2(int? count = null)
        {
            List<PublisherEntity2> publisherEntities = new List<PublisherEntity2>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherEntities.Add(new PublisherEntity2
                {
                    Type = (PublisherType)random.Next(1, 3),
                    Name = "Publisher" + i.ToString()
                });
            }

            return publisherEntities;
        }
    }
}
