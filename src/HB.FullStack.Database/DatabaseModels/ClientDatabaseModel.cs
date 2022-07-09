using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common.IdGen;

namespace HB.FullStack.Database.DatabaseModels
{
    public abstract class ClientDatabaseModel : DatabaseModel
    {

    }

    public abstract class ClientLongIdDatabaseModel : ClientDatabaseModel
    {
        [DatabaseModelProperty(0)]
        public abstract long Id { get; set; }
    }

    public abstract class ClientAutoIncrementIdDatabaseModel : ClientLongIdDatabaseModel
    {
        [AutoIncrementPrimaryKey]
        [DatabaseModelProperty(0)]
        [CacheKey]
        public override long Id { get; set; } = -1;
    }

    public abstract class ClientFlackIdDatabaseModel : ClientLongIdDatabaseModel
    {
        [PrimaryKey]
        [DatabaseModelProperty(0)]
        [CacheKey]
        [LongId2]
        public override long Id { get; set; } = StaticIdGen.GetId();
    }

    public abstract class ClientGuidDatabaseModel : ClientDatabaseModel
    {
        [DatabaseModelProperty(0)]
        [NoEmptyGuid]
        [PrimaryKey]
        [CacheKey]
        public Guid Id { get; set; } = SecurityUtil.CreateSequentialGuid(DateTimeOffset.UtcNow, GuidStoredFormat.AsBinary);

        //TODO: 这里是按AsBinary来生成的，在不同的数据库需要不同的生成
    }
}