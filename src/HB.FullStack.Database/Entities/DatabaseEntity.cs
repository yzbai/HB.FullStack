using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.IdGen;

using MessagePack;

using Key = MessagePack.KeyAttribute;

namespace HB.FullStack.Database.Entities
{
    [MessagePackObject]
    public abstract class DatabaseEntity : Entity
    {
        [Key(5)]
        public override string LastUser { get; set; } = string.Empty;
    }

    [MessagePackObject]
    public abstract class LongIdEntity : DatabaseEntity
    {
        [IgnoreMember]
        public abstract long Id { get; set; }
    }

    [MessagePackObject]
    public abstract class AutoIncrementIdEntity : LongIdEntity
    {
        [AutoIncrementPrimaryKey]
        [EntityProperty(0)]
        [CacheKey]
        [Key(6)]
        public override long Id { get; set; } = -1;
    }

    [MessagePackObject]
    public abstract class FlackIdEntity : LongIdEntity
    {
        [PrimaryKey]
        [EntityProperty(0)]
        [CacheKey]
        [LongId2]
        [Key(6)]
        public override long Id { get; set; } = StaticIdGen.GetId();
    }

    [MessagePackObject]
    public abstract class GuidEntity : DatabaseEntity
    {
        [NoEmptyGuid]
        [PrimaryKey]
        [CacheKey]
        [Key(6)]
        public Guid Id { get; set; } = SecurityUtil.CreateSequentialGuid(DateTimeOffset.UtcNow, GuidStoredFormat.AsBinary);

        //TODO: 这里是按AsBinary来生成的，在不同的数据库需要不同的生成
    }
}
