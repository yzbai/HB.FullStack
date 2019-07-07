using System;
using System.Collections.Generic;

namespace DatabaseSample
{
    internal class MokeData
    {
        internal static IList<BookEntity> GetBooks()
        {
            Random random = new Random();

            List<BookEntity> lst = new List<BookEntity>();

            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });
            lst.Add(new BookEntity { Guid = SecurityUtil.CreateUniqueToken(), Name = "Name" + random.Next(10000), Price = random.NextDouble(), Nonsence = random.NextDouble().ToString() });

            return lst;

        }
    }
}