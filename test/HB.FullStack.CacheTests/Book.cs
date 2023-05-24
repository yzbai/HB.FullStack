using System;
using System.Collections.Generic;

using HB.FullStack.Common;
using HB.FullStack.Common.IdGen;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.CacheTests
{
    [CacheModel]
    public class Book : DbModel<long>, ITimestamp
    {
        [DbField]
        [CacheModelAltKey]
        public string Name { get; set; } = null!;

        [DbField]
        [CacheModelAltKey]
        public long BookID { get; set; }

        [DbField]
        public string? Publisher { get; set; }

        [DbField]
        public double Price { get; set; }

        public override long Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        public long Timestamp { get; set; }
    }

    internal static partial class Mocker
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
    }
}