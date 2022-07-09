using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.Cache.CacheModels;
using HB.FullStack.Common.IdGen;

namespace HB.FullStack.Database.DatabaseModels
{
    public abstract class ServerDatabaseModel : DatabaseModel, ICacheModel
    {
        public int Version { get; set; } = -1;

        public string LastUser { get; set; } = string.Empty;

        public DateTimeOffset LastTime { get; set; } = TimeUtil.UtcNow;
    }

    public abstract class LongIdDatabaseModel : ServerDatabaseModel
    {
        [DatabaseModelProperty(0)]
        public abstract long Id { get; set; }
    }

    public abstract class AutoIncrementIdDatabaseModel : LongIdDatabaseModel
    {
        [AutoIncrementPrimaryKey]
        [DatabaseModelProperty(0)]
        [CacheKey]
        public override long Id { get; set; } = -1;
    }

    public abstract class FlackIdDatabaseModel : LongIdDatabaseModel
    {
        [PrimaryKey]
        [DatabaseModelProperty(0)]
        [CacheKey]
        [LongId2]
        public override long Id { get; set; } = StaticIdGen.GetId();
    }

    public abstract class GuidDatabaseModel : ServerDatabaseModel
    {
        [DatabaseModelProperty(0)]
        [NoEmptyGuid]
        [PrimaryKey]
        [CacheKey]
        public Guid Id { get; set; } = SecurityUtil.CreateSequentialGuid(DateTimeOffset.UtcNow, GuidStoredFormat.AsBinary);

        //TODO: 这里是按AsBinary来生成的，在不同的数据库需要不同的生成
    }

}