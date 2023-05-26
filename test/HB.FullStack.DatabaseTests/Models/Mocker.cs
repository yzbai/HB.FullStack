using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using HB.FullStack.BaseTest.Models;
using HB.FullStack.Common.IdGen;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.DatabaseTests
{
    internal static partial class Mocker
    {
        private static readonly Random _random = new Random();

        internal static T MockOne<T>(Action<T>? additionalAction = null) where T : IDbModel => Mock<T>(1, (i, t) => additionalAction?.Invoke(t)).First();

        internal static IList<T> Mock<T>(int count, Action<int, T>? additionalAction = null) where T : IDbModel
        {
            var results = new List<T>();

            for (int i = 0; i < count; ++i)
            {
                var t = (T)Activator.CreateInstance(typeof(T))!;

                if (t is IPublisherModel publisher)
                {
                    publisher.Type = (PublisherType)(i % 3);
                    publisher.Type2 = i % 2 == 0 ? (PublisherType)(i % 3) : null;
                    publisher.Name = "中文名字" + StaticIdGen.GetLongId();
                    publisher.Name2 = i % 2 == 0 ? null : "中文名字2_" + StaticIdGen.GetLongId();
                    publisher.Books = ImmutableList.Create("Cat", "Dog");
                    publisher.BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } }.ToImmutableDictionary();
                    publisher.BookAuthors = new Dictionary<string, Author>()
                    {
                        { "Cat", new Author() { Mobile="111", Name="BB" } },
                        { "Dog", new Author() { Mobile="222", Name="sx" } }
                    }.ToImmutableDictionary();

                    publisher.Number = _random.Next(1, 100);
                    publisher.Number1 = i % 2 == 0 ? _random.Next(1, 100) : null;

                    publisher.DDD = i % 2 == 0 ? null : TimeUtil.UtcNow;

                    publisher.Float = (float)_random.NextDouble();
                    publisher.Float2 = i % 2 == 0 ? null : _random.NextDouble();

                    publisher.TimeOnly = TimeUtil.UtcTimeOnlyNow;
                    publisher.DateOnly = TimeUtil.UtcDateOnlyNow;

                    publisher.NullableTimeOnly = i % 2 == 0 ? null : TimeUtil.UtcTimeOnlyNow;
                    publisher.NullableDateOnly = i % 2 == 0 ? null : TimeUtil.UtcDateOnlyNow;

                }
                else if (t is IBookModel book)
                {
                    book.Name = "Book" + i.ToString();
                    book.Price = _random.NextDouble();
                }
                else if (t is IJoinTest_A a)
                {

                }
                else if (t is IJoinTest_B b)
                {

                }
                else if (t is IJoinTest_AB ab)
                {

                }
                else if (t is IJoinTest_A_Sub aSub)
                {

                }

                additionalAction?.Invoke(i, t);

                results.Add(t);
            }

            return results;
        }

        internal static void Modify<T>(T model, Action<T>? additionalAction = null) where T : IDbModel
        {
            if (model is IPublisherModel publisher)
            {
                //model.Guid = Guid.NewGuid().ToString();
                publisher.Type = PublisherType.Online;
                publisher.Name += "Updated";
                publisher.Books = new List<string>() { "xxx", "tttt" }.ToImmutableList();
                publisher.BookAuthors = new Dictionary<string, Author>()
                {
                    { "Cat", new Author() { Mobile = "111", Name = "BB" } },
                    { "Dog", new Author() { Mobile = "222", Name = "sx" } }
                }.ToImmutableDictionary();
            }
            else if (model is IBookModel book)
            {
                book.Name += "Updated";
            }
            else
            {
                throw new NotImplementedException();
            }

            if (additionalAction != null)
            {
                additionalAction(model);
            }
        }

        internal static void Modify<T>(IList<T> models, Action<T>? additionalAction = null) where T : IDbModel
        {
            foreach (var model in models)
            {
                Modify(model, additionalAction);
            }
        }

        public static IList<IPublisherModel> GetPublishModel(DbEngineType engineType, bool isTimestamp, DbModelIdType idType, int? count = null)
        {
            count ??= 50;
            List<IPublisherModel> publishers = new List<IPublisherModel>();

            Func<IPublisherModel> createNew;

            if (engineType == DbEngineType.MySQL)
            {
                if (isTimestamp)
                {
                    createNew = idType switch
                    {
                        DbModelIdType.Unkown => throw new NotImplementedException(),
                        DbModelIdType.LongId => () => new MySql_Timestamp_Long_PublisherModel(),
                        DbModelIdType.AutoIncrementLongId => () => new MySql_Timestamp_Long_AutoIncrementId_PublisherModel(),
                        DbModelIdType.GuidId => () => new MySql_Timestamp_Guid_PublisherModel(),
                        DbModelIdType.StringId => throw new NotImplementedException(),
                        _ => throw new NotImplementedException(),
                    };
                }
                else
                {
                    createNew = idType switch
                    {
                        DbModelIdType.Unkown => throw new NotImplementedException(),
                        DbModelIdType.LongId => () => new MySql_Timeless_Long_PublisherModel(),
                        DbModelIdType.AutoIncrementLongId => () => new MySql_Timeless_Long_AutoIncrementId_PublisherModel(),
                        DbModelIdType.GuidId => () => new MySql_Timeless_Guid_PublisherModel(),
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
                        DbModelIdType.LongId => () => new Sqlite_Timestamp_Long_PublisherModel(),
                        DbModelIdType.AutoIncrementLongId => () => new Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel(),
                        DbModelIdType.GuidId => () => new Sqlite_Timestamp_Guid_PublisherModel(),
                        DbModelIdType.StringId => throw new NotImplementedException(),
                        _ => throw new NotImplementedException(),
                    };
                }
                else
                {
                    createNew = idType switch
                    {
                        DbModelIdType.Unkown => throw new NotImplementedException(),
                        DbModelIdType.LongId => () => new Sqlite_Timeless_Long_PublisherModel(),
                        DbModelIdType.AutoIncrementLongId => () => new Sqlite_Timeless_Long_AutoIncrementId_PublisherModel(),
                        DbModelIdType.GuidId => () => new Sqlite_Timeless_Guid_PublisherModel(),
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
                var publisher = createNew();

                publisher.Type = (PublisherType)(i % 3);
                publisher.Type2 = i % 2 == 0 ? (PublisherType)(i % 3) : null;
                publisher.Name = "中文名字" + StaticIdGen.GetLongId();
                publisher.Name2 = i % 2 == 0 ? null : "中文名字2_" + StaticIdGen.GetLongId();
                publisher.Books = ImmutableList.Create("Cat", "Dog");
                publisher.BookNames = new Dictionary<string, string> { { "a", "b" }, { "c", "d" } }.ToImmutableDictionary();
                publisher.BookAuthors = new Dictionary<string, Author>()
                {
                    { "Cat", new Author() { Mobile="111", Name="BB" } },
                    { "Dog", new Author() { Mobile="222", Name="sx" } }
                }.ToImmutableDictionary();

                publisher.Number = _random.Next(1, 100);
                publisher.Number1 = i % 2 == 0 ? _random.Next(1, 100) : null;

                publisher.DDD = i % 2 == 0 ? null : TimeUtil.UtcNow;

                publisher.Float = (float)_random.NextDouble();
                publisher.Float2 = i % 2 == 0 ? null : _random.NextDouble();

                publishers.Add(publisher);
            }

            return publishers;
        }
    }
}