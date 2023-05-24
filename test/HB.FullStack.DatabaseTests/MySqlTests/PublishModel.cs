using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using HB.FullStack.Common;
using HB.FullStack.Common.IdGen;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.DatabaseTests.MySqlTests
{
    public enum PublisherType
    {
        Online = 0,
        Big = 1,
        Small = 2
    }

    public class Author
    {
        public string Name { get; set; } = default!;

        public string Mobile { get; set; } = default!;
    }

    public interface IPublisherModel
    {
        IImmutableDictionary<string, Author>? BookAuthors { get; set; }
        IImmutableDictionary<string, string>? BookNames { get; set; }
        IImmutableList<string>? Books { get; set; }
        DateTimeOffset? DDD { get; set; }
        string Name { get; set; }
        string? Name2 { get; set; }
        int Number { get; set; }
        int? Number1 { get; set; }
        PublisherType Type { get; set; }
        PublisherType? Type2 { get; set; }
        float Float { get; set; }

        double? Float2 { get; set; }
    }

    [PropertyTrackableObject]
    public abstract partial class PublisherModel<TId> : DbModel<TId>, IPublisherModel
    {
        [TrackProperty]
        string _name = null!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        [TrackProperty]
        IImmutableList<string>? _books;

        [TrackProperty]
        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        IImmutableDictionary<string, Author>? _bookAuthors;

        [TrackProperty]
        [DbField(MaxLength = DbSchema.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        IImmutableDictionary<string, string>? _bookNames;

        [TrackProperty]
        PublisherType _type;

        [TrackProperty]
        float _float = 1.9877f;

        [TrackProperty]
        double? _float2;

        [TrackProperty]
        int _number;

        [TrackProperty]
        int? _number1;

        [TrackProperty]
        PublisherType? _type2;

        [TrackProperty]
        [DbField(MaxLength = 20)]
        string? _name2;

        [TrackProperty]
        DateTimeOffset? _dDD;

        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Timestamp_Long_PublisherModel : PublisherModel<long>, ITimestamp
    {
        public override long Id { get; set; }
        public long Timestamp { get; set; }
    }


    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Timestamp_Long_AutoIncrementId_PublisherModel : PublisherModel<long>, ITimestamp
    {
        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }
        public long Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Timestamp_Guid_PublisherModel : PublisherModel<Guid>, ITimestamp
    {
        public override Guid Id { get; set; }
        public long Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Timeless_Long_PublisherModel : PublisherModel<long>
    {
        public override long Id { get; set; }
    }


    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Timeless_Long_AutoIncrementId_PublisherModel : PublisherModel<long>
    {

        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class MySql_Timeless_Guid_PublisherModel : PublisherModel<Guid>
    {

        public override Guid Id { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class Sqlite_Timestamp_Long_PublisherModel : PublisherModel<long>, ITimestamp
    {
        public override long Id { get; set; }
        public long Timestamp { get; set; }
    }


    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel : PublisherModel<long>, ITimestamp
    {
        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }
        public long Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class Sqlite_Timestamp_Guid_PublisherModel : PublisherModel<Guid>, ITimestamp
    {
        public override Guid Id { get; set; }
        public long Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class Sqlite_Timeless_Long_PublisherModel : PublisherModel<long>
    {
        public override long Id { get; set; }
    }


    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class Sqlite_Timeless_Long_AutoIncrementId_PublisherModel : PublisherModel<long>
    {

        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Mysql)]
    public class Sqlite_Timeless_Guid_PublisherModel : PublisherModel<Guid>
    {

        public override Guid Id { get; set; }
    }

    internal partial class Mocker3
    {
        private static Random _random = new Random();

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