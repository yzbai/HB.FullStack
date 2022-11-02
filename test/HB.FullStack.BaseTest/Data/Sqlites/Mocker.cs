using System;
using System.Collections.Generic;
using System.Text;

namespace HB.FullStack.BaseTest.Data.Sqlites
{
    public static partial class Mocker
    {
        private static readonly Random _random = new Random();

        public static IList<AutoIdBTTimestamp> GetAutoIdBTTimestamps(int count)
        {
            List<AutoIdBTTimestamp> lst = new List<AutoIdBTTimestamp>();

            for (int i = 0; i < count; ++i)
            {
                lst.Add(new AutoIdBTTimestamp());
            }

            return lst;
        }

        public static IList<AutoIdBTTimeless> GetAutoIdBTTimelesses(int count)
        {
            List<AutoIdBTTimeless> lst = new List<AutoIdBTTimeless>();

            for (int i = 0; i < count; ++i)
            {
                lst.Add(new AutoIdBTTimeless());
            }

            return lst;
        }

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

        public static IList<Book2Model> GetBooks(int? count = null)
        {
            List<Book2Model> books = new List<Book2Model>();

            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                books.Add(new Book2Model
                {
                    //Guid = SecurityUtil.CreateUniqueToken(),
                    Name = "Book" + i.ToString(),
                    Price = _random.NextDouble()
                });
            }

            return books;
        }

        public static IList<Guid_BookModel> Guid_GetBooks(int? count = null)
        {
            List<Guid_BookModel> books = new List<Guid_BookModel>();

            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                books.Add(new Guid_BookModel
                {
                    //Guid = SecurityUtil.CreateUniqueToken(),
                    Name = "Book" + i.ToString(),
                    Price = _random.NextDouble()
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
                    //Guid = SecurityUtil.CreateUniqueToken(),
                    Name = "Book" + i.ToString(),
                    Price = _random.NextDouble()
                });
            }

            return books;
        }

        public static PublisherModel MockOnePublisherModel()
        {
            PublisherModel model = new PublisherModel
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

            return model;
        }

        public static Guid_PublisherModel Guid_MockOnePublisherModel()
        {
            Guid_PublisherModel model = new Guid_PublisherModel
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

            return model;
        }

        public static PublisherModel_Client MockOnePublisherModel_Client()
        {
            PublisherModel_Client model = new PublisherModel_Client
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

            return model;
        }

        public static Guid_PublisherModel_Client Guid_MockOnePublisherModel_Client()
        {
            Guid_PublisherModel_Client model = new Guid_PublisherModel_Client
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

            return model;
        }

        public static List<PublisherModel> GetPublishers(int? count = null)
        {
            List<PublisherModel> publisherModels = new List<PublisherModel>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherModels.Add(new PublisherModel
                {
                    Books = new List<string> { "a", "v", "c" },
                    Type = (PublisherType)random.Next(0, 3),
                    Name = "Publisher" + i.ToString(),
                    //BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                    BookAuthors = new Dictionary<string, Author> { { "a", new Author { Mobile = "xxxx", Name = "tttt" } }, { "xxx", new Author { Mobile = "gggg", Name = "safas" } } }
                });
            }

            return publisherModels;
        }

        public static List<Guid_PublisherModel> Guid_GetPublishers(int? count = null)
        {
            List<Guid_PublisherModel> publisherModels = new List<Guid_PublisherModel>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherModels.Add(new Guid_PublisherModel
                {
                    Books = new List<string> { "a", "v", "c" },
                    Type = (PublisherType)random.Next(0, 3),
                    Name = "Publisher" + i.ToString(),
                    //BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                    BookAuthors = new Dictionary<string, Author> { { "a", new Author { Mobile = "xxxx", Name = "tttt" } }, { "xxx", new Author { Mobile = "gggg", Name = "safas" } } }
                });
            }

            return publisherModels;
        }

        public static List<PublisherModel_Client> GetPublishers_Client(int? count = null)
        {
            List<PublisherModel_Client> publisherModels = new List<PublisherModel_Client>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherModels.Add(new PublisherModel_Client
                {
                    Books = new List<string> { "a", "v", "c" },
                    Type = (PublisherType)random.Next(0, 3),
                    Name = "Publisher" + i.ToString(),
                    //BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                    BookAuthors = new Dictionary<string, Author> { { "a", new Author { Mobile = "xxxx", Name = "tttt" } }, { "xxx", new Author { Mobile = "gggg", Name = "safas" } } }
                });
            }

            return publisherModels;
        }

        public static List<Guid_PublisherModel_Client> Guid_GetPublishers_Client(int? count = null)
        {
            List<Guid_PublisherModel_Client> publisherModels = new List<Guid_PublisherModel_Client>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherModels.Add(new Guid_PublisherModel_Client
                {
                    Books = new List<string> { "a", "v", "c" },
                    Type = (PublisherType)random.Next(0, 3),
                    Name = "Publisher" + i.ToString(),
                    //BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } },
                    BookAuthors = new Dictionary<string, Author> { { "a", new Author { Mobile = "xxxx", Name = "tttt" } }, { "xxx", new Author { Mobile = "gggg", Name = "safas" } } }
                });
            }

            return publisherModels;
        }

        public static IList<PublisherModel2> GetPublishers2(int? count = null)
        {
            List<PublisherModel2> publisherModels = new List<PublisherModel2>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherModels.Add(new PublisherModel2
                {
                    Type = (PublisherType)random.Next(1, 3),
                    Name = "Publisher" + i.ToString()
                });
            }

            return publisherModels;
        }

        public static IList<Guid_PublisherModel2> Guid_GetPublishers2(int? count = null)
        {
            List<Guid_PublisherModel2> publisherModels = new List<Guid_PublisherModel2>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherModels.Add(new Guid_PublisherModel2
                {
                    Type = (PublisherType)random.Next(1, 3),
                    Name = "Publisher" + i.ToString()
                });
            }

            return publisherModels;
        }

        public static IList<PublisherModel2_Client> GetPublishers2_Client(int? count = null)
        {
            List<PublisherModel2_Client> publisherModels = new List<PublisherModel2_Client>();

            Random random = new Random();
            int length = count == null ? 50 : count.Value;

            for (int i = 0; i < length; ++i)
            {
                publisherModels.Add(new PublisherModel2_Client
                {
                    Type = (PublisherType)random.Next(1, 3),
                    Name = "Publisher" + i.ToString()
                });
            }

            return publisherModels;
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