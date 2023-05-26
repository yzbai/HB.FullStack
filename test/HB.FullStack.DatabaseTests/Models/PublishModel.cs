using System;
using System.Collections.Immutable;

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;
using HB.FullStack.Database.Config;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.DbModels;

namespace HB.FullStack.DatabaseTests
{
    public interface IPublisherModel : IDbModel
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

        DateOnly DateOnly { get; set; }

        TimeOnly TimeOnly { get; set; }

        DateOnly? NullableDateOnly { get; set; }

        TimeOnly? NullableTimeOnly { get; set; }
    }

    [PropertyTrackableObject]
    public abstract partial class PublisherModel<TId> : DbModel<TId>, IPublisherModel
    {
        [TrackProperty]
        private string _name = null!;

        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        [TrackProperty]
        private IImmutableList<string>? _books;

        [TrackProperty]
        [DbField(Converter = typeof(JsonDbPropertyConverter))]
        private IImmutableDictionary<string, Author>? _bookAuthors;

        [TrackProperty]
        [DbField(MaxLength = DbSchema.MAX_VARCHAR_LENGTH / 2, Converter = typeof(JsonDbPropertyConverter))]
        private IImmutableDictionary<string, string>? _bookNames;

        [TrackProperty]
        private PublisherType _type;

        [TrackProperty]
        private float _float = 1.9877f;

        [TrackProperty]
        private double? _float2;

        [TrackProperty]
        private int _number;

        [TrackProperty]
        private int? _number1;

        [TrackProperty]
        private PublisherType? _type2;

        [TrackProperty]
        [DbField(MaxLength = 20)]
        private string? _name2;

        [TrackProperty]
        private DateTimeOffset? _dDD;

        [TrackProperty]
        private DateOnly _dateOnly;

        [TrackProperty]
        private TimeOnly _timeOnly;

        [TrackProperty]
        private DateOnly? _nullableDateOnly;

        [TrackProperty]
        private TimeOnly? _nullableTimeOnly;

        public override bool Deleted { get; set; }
        public override string? LastUser { get; set; }
        //object IPublisherModel.Id { get => Id!; set => Id = (TId)value; }
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

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Timestamp_Long_PublisherModel : PublisherModel<long>, ITimestamp
    {
        public override long Id { get; set; }
        public long Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Timestamp_Long_AutoIncrementId_PublisherModel : PublisherModel<long>, ITimestamp
    {
        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }

        public long Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Timestamp_Guid_PublisherModel : PublisherModel<Guid>, ITimestamp
    {
        public override Guid Id { get; set; }
        public long Timestamp { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Timeless_Long_PublisherModel : PublisherModel<long>
    {
        public override long Id { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Timeless_Long_AutoIncrementId_PublisherModel : PublisherModel<long>
    {
        [DbAutoIncrementPrimaryKey]
        public override long Id { get; set; }
    }

    [DbModel(DbSchemaName = BaseTestClass.DbSchema_Sqlite)]
    public class Sqlite_Timeless_Guid_PublisherModel : PublisherModel<Guid>
    {
        public override Guid Id { get; set; }
    }

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
}