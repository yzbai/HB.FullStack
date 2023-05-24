using System;
using System.Collections.Generic;

using HB.FullStack.Common;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.DatabaseTests.MySqlTests;

namespace HB.FullStack.DatabaseTests
{
    public interface ITimestamp_Guid_BookModel : IDbModel, ITimestamp
    {
        string Name { get; set; }
        double Price { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_ITimestamp_Guid_BookModel : DbModel<Guid>, ITimestamp_Guid_BookModel
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;

        [DbForeignKey(typeof(MySql_Timestamp_Guid_PublisherModel), true)]
        public Guid PublisherId { get; set; }
        
        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }

        long ITimestamp.Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_ITimestamp_Guid_BookModel : DbModel<Guid>, ITimestamp_Guid_BookModel
    {
        [DbField]
        public string Name { get; set; } = default!;

        [DbField]
        public double Price { get; set; } = default!;

        [DbForeignKey(typeof(MySql_Timestamp_Guid_PublisherModel), true)]
        public Guid PublisherId { get; set; }

        public override Guid Id { get; set; }
        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }

        long ITimestamp.Timestamp { get; set; }
    }

    internal partial class Mocker3
    {
        private static Random _random = new Random();

        public static IList<ITimestamp_Guid_BookModel> GetBooks(DbEngineType engineType, int? count = null)
        {
            List<ITimestamp_Guid_BookModel> books = new List<ITimestamp_Guid_BookModel>();

            int length = count == null ? 50 : count.Value;

            Func<int, ITimestamp_Guid_BookModel> createNew = engineType switch
            {
                DbEngineType.MySQL => i => new MySql_ITimestamp_Guid_BookModel
                {
                    Name = "Book" + i.ToString(),
                    Price = _random.NextDouble()
                },
                DbEngineType.SQLite => i => new Sqlite_ITimestamp_Guid_BookModel
                {
                    Name = "Book" + i.ToString(),
                    Price = _random.NextDouble()
                },
                _ => throw new NotImplementedException(),
            };

            for (int i = 0; i < length; ++i)
            {
                books.Add(createNew(i));
            }

            return books;
        }
    }
}