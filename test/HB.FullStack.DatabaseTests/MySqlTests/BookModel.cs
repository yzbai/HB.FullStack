using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

using HB.FullStack.Common;
using HB.FullStack.Common.IdGen;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;
using HB.FullStack.DatabaseTests.MySqlTests;

namespace HB.FullStack.DatabaseTests
{
    public interface IBookModel : IModel
    {
        object Id { get; set; }
        string Name { get; set; }

        double Price { get; set; }

        //TODO:思考，是否在IBookModel上加一个Attribute来表明与Publisher的关系即可？
        object? PublisherId { get; set; }
    }

    public abstract partial class BookModel<TId> : DbModel<TId>, IBookModel
    {
        public string Name { get; set; } = null!;
        public double Price { get; set; }

        public abstract TId? PublisherId { get; set; }

        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        object? IBookModel.PublisherId { get => PublisherId; set => PublisherId = (TId?)value; }
        object IBookModel.Id { get => Id!; set => Id = (TId)value; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class MySql_Timestamp_Guid_BookModel : BookModel<Guid>, ITimestamp
    {
        public override Guid Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Guid_PublisherModel), true)]
        public override Guid PublisherId { get; set; }


        long ITimestamp.Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class MySql_Timestamp_Long_BookModel : BookModel<long>, ITimestamp
    {
        public override long Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Long_PublisherModel), true)]
        public override long PublisherId { get; set; }


        long ITimestamp.Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class MySql_Timestamp_Long_AutoIncrementId_BookModel : BookModel<long>, ITimestamp
    {
        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Long_AutoIncrementId_PublisherModel), true)]
        public override long PublisherId { get; set; }


        long ITimestamp.Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class MySql_Timeless_Guid_BookModel : BookModel<Guid>
    {
        public override Guid Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Guid_PublisherModel), true)]
        public override Guid PublisherId { get; set; }


    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class MySql_Timeless_Long_BookModel : BookModel<long>
    {
        public override long Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Long_PublisherModel), true)]
        public override long PublisherId { get; set; }

    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class MySql_Timeless_Long_AutoIncrementId_BookModel : BookModel<long>
    {
        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Long_AutoIncrementId_PublisherModel), true)]
        public override long PublisherId { get; set; }
    }
    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class Sqlite_Timestamp_Guid_BookModel : BookModel<Guid>, ITimestamp
    {
        public override Guid Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Guid_PublisherModel), true)]
        public override Guid PublisherId { get; set; }


        long ITimestamp.Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class Sqlite_Timestamp_Long_BookModel : BookModel<long>, ITimestamp
    {
        public override long Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Long_PublisherModel), true)]
        public override long PublisherId { get; set; }


        long ITimestamp.Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class Sqlite_Timestamp_Long_AutoIncrementId_BookModel : BookModel<long>, ITimestamp
    {
        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Long_AutoIncrementId_PublisherModel), true)]
        public override long PublisherId { get; set; }


        long ITimestamp.Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class Sqlite_Timeless_Guid_BookModel : BookModel<Guid>
    {
        public override Guid Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Guid_PublisherModel), true)]
        public override Guid PublisherId { get; set; }


    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class Sqlite_Timeless_Long_BookModel : BookModel<long>
    {
        public override long Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Long_PublisherModel), true)]
        public override long PublisherId { get; set; }

    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite, ConflictCheckMethods = ConflictCheckMethods.Ignore | ConflictCheckMethods.OldNewValueCompare | ConflictCheckMethods.Timestamp)]
    public class Sqlite_Timeless_Long_AutoIncrementId_BookModel : BookModel<long>
    {
        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }


        [DbForeignKey(typeof(MySql_Timestamp_Long_AutoIncrementId_PublisherModel), true)]
        public override long PublisherId { get; set; }
    }

    internal static partial class Mocker
    {
        private static Random _random = new Random();

        public static IList<IBookModel> GetBookModel(DbEngineType engineType, bool isTimestamp, DbModelIdType idType, object? publisherId, int? count = null)
        {
            count ??= 50;
            List<IBookModel> books = new List<IBookModel>();

            Func<IBookModel> createNew;

            if (engineType == DbEngineType.MySQL)
            {
                if (isTimestamp)
                {
                    createNew = idType switch
                    {
                        DbModelIdType.Unkown => throw new NotImplementedException(),
                        DbModelIdType.LongId => () => new MySql_Timestamp_Long_BookModel(),
                        DbModelIdType.AutoIncrementLongId => () => new MySql_Timestamp_Long_AutoIncrementId_BookModel(),
                        DbModelIdType.GuidId => () => new MySql_Timestamp_Guid_BookModel(),
                        DbModelIdType.StringId => throw new NotImplementedException(),
                        _ => throw new NotImplementedException(),
                    };
                }
                else
                {
                    createNew = idType switch
                    {
                        DbModelIdType.Unkown => throw new NotImplementedException(),
                        DbModelIdType.LongId => () => new MySql_Timeless_Long_BookModel(),
                        DbModelIdType.AutoIncrementLongId => () => new MySql_Timeless_Long_AutoIncrementId_BookModel(),
                        DbModelIdType.GuidId => () => new MySql_Timeless_Guid_BookModel(),
                        DbModelIdType.StringId => throw new NotImplementedException(),
                        _ => throw new NotImplementedException(),
                    };
                }
            }
            else if (engineType == DbEngineType.SQLite)
            {
                if (isTimestamp)
                {
                    createNew = idType switch
                    {
                        DbModelIdType.Unkown => throw new NotImplementedException(),
                        DbModelIdType.LongId => () => new Sqlite_Timestamp_Long_BookModel(),
                        DbModelIdType.AutoIncrementLongId => () => new Sqlite_Timestamp_Long_AutoIncrementId_BookModel(),
                        DbModelIdType.GuidId => () => new Sqlite_Timestamp_Guid_BookModel(),
                        DbModelIdType.StringId => throw new NotImplementedException(),
                        _ => throw new NotImplementedException(),
                    };
                }
                else
                {
                    createNew = idType switch
                    {
                        DbModelIdType.Unkown => throw new NotImplementedException(),
                        DbModelIdType.LongId => () => new Sqlite_Timeless_Long_BookModel(),
                        DbModelIdType.AutoIncrementLongId => () => new Sqlite_Timeless_Long_AutoIncrementId_BookModel(),
                        DbModelIdType.GuidId => () => new Sqlite_Timeless_Guid_BookModel(),
                        DbModelIdType.StringId => throw new NotImplementedException(),
                        _ => throw new NotImplementedException(),
                    };
                }
            }
            else
            {
                throw new NotSupportedException();
            }


            for (int i = 0; i < count; ++i)
            {
                var book = createNew();

                book.Name = "Book" + i.ToString();
                book.Price = _random.NextDouble();
                book.PublisherId = publisherId;

                books.Add(book);
            }

            return books;
        }
    }
}