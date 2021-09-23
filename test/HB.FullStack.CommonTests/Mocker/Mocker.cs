using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.DatabaseTests.Data
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

        public static PublisherEntity MockOnePublisherEntity()
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


        public static Guid_PublisherEntity Guid_MockOnePublisherEntity()
        {
            Guid_PublisherEntity entity = new Guid_PublisherEntity
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

        internal static PublisherEntity_Client MockOnePublisherEntity_Client()
        {
            PublisherEntity_Client entity = new PublisherEntity_Client
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

        public static Guid_PublisherEntity_Client Guid_MockOnePublisherEntity_Client()
        {
            Guid_PublisherEntity_Client entity = new Guid_PublisherEntity_Client
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

        public static List<PublisherEntity> GetPublishers(int? count = null)
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
                    //BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                    BookAuthors = new Dictionary<string, Author> { { "a", new Author { Mobile = "xxxx", Name = "tttt" } }, { "xxx", new Author { Mobile = "gggg", Name = "safas" } } }
                });
            }

            return publisherEntities;
        }

        public static List<Guid_PublisherEntity> Guid_GetPublishers(int? count = null)
        {
            List<Guid_PublisherEntity> publisherEntities = new List<Guid_PublisherEntity>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherEntities.Add(new Guid_PublisherEntity
                {
                    Books = new List<string> { "a", "v", "c" },
                    Type = (PublisherType)random.Next(0, 3),
                    Name = "Publisher" + i.ToString(),
                    //BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                    BookAuthors = new Dictionary<string, Author> { { "a", new Author { Mobile = "xxxx", Name = "tttt" } }, { "xxx", new Author { Mobile = "gggg", Name = "safas" } } }
                });
            }

            return publisherEntities;
        }

        internal static List<PublisherEntity_Client> GetPublishers_Client(int? count = null)
        {
            List<PublisherEntity_Client> publisherEntities = new List<PublisherEntity_Client>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherEntities.Add(new PublisherEntity_Client
                {
                    Books = new List<string> { "a", "v", "c" },
                    Type = (PublisherType)random.Next(0, 3),
                    Name = "Publisher" + i.ToString(),
                    //BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                    BookAuthors = new Dictionary<string, Author> { { "a", new Author { Mobile = "xxxx", Name = "tttt" } }, { "xxx", new Author { Mobile = "gggg", Name = "safas" } } }
                });
            }

            return publisherEntities;
        }

        public static List<Guid_PublisherEntity_Client> Guid_GetPublishers_Client(int? count = null)
        {
            List<Guid_PublisherEntity_Client> publisherEntities = new List<Guid_PublisherEntity_Client>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherEntities.Add(new Guid_PublisherEntity_Client
                {
                    Books = new List<string> { "a", "v", "c" },
                    Type = (PublisherType)random.Next(0, 3),
                    Name = "Publisher" + i.ToString(),
                    //BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
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

        public static IList<Guid_PublisherEntity2> Guid_GetPublishers2(int? count = null)
        {
            List<Guid_PublisherEntity2> publisherEntities = new List<Guid_PublisherEntity2>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherEntities.Add(new Guid_PublisherEntity2
                {
                    Type = (PublisherType)random.Next(1, 3),
                    Name = "Publisher" + i.ToString()
                });
            }

            return publisherEntities;
        }

        public static IList<PublisherEntity2_Client> GetPublishers2_Client(int? count = null)
        {
            List<PublisherEntity2_Client> publisherEntities = new List<PublisherEntity2_Client>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherEntities.Add(new PublisherEntity2_Client
                {
                    Type = (PublisherType)random.Next(1, 3),
                    Name = "Publisher" + i.ToString()
                });
            }

            return publisherEntities;
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
