using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Database.Test.Data
{
    public class Mocker
    {
        private static Random random = new Random();

        public static IList<BookEntity> GetBooks()
        {
            List<BookEntity> books = new List<BookEntity>();

            for (int i = 0; i < 5; ++i)
            {
                books.Add(new BookEntity {
                    Guid = SecurityUtil.CreateUniqueToken(),
                    Name = "Book" + i.ToString(),
                    Price = random.NextDouble()
                });
            }

            return books;
        }

        public static PublisherEntity MockOne()
        {
            PublisherEntity entity = new PublisherEntity();
            entity.Guid = SecurityUtil.CreateUniqueToken();
            entity.Type = PublisherType.Online;
            entity.Name = "中文名字";
            entity.Books = new List<string>() { "Cat", "Dog" };
            entity.BookAuthors = new Dictionary<string, Author>()
            {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                };

            return entity;
        }

        public static IList<PublisherEntity> GetPublishers()
        {
            List<PublisherEntity> publisherEntities = new List<PublisherEntity>();

            for (int i = 0; i < 5; ++i)
            {
                publisherEntities.Add(new PublisherEntity {
                    Guid = SecurityUtil.CreateUniqueToken(),
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
