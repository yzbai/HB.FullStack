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

namespace HB.FullStack.Database.Entities
{
    public abstract class DatabaseEntity : Entity
    {
        public override string LastUser { get; set; } = string.Empty;
    }

    public abstract class LongIdEntity : DatabaseEntity
    {
        public abstract long Id { get; set; }
    }

    public abstract class AutoIncrementIdEntity : LongIdEntity
    {
        [AutoIncrementPrimaryKey]
        [EntityProperty(0)]
        [CacheKey]
        public override long Id { get; set; } = -1;
    }

    public abstract class FlackIdEntity : LongIdEntity
    {
        [PrimaryKey]
        [EntityProperty(0)]
        [CacheKey]
        [LongId2]
        public override long Id { get; set; } = StaticIdGen.GetId();
    }

    public abstract class GuidEntity : DatabaseEntity
    {
        [NoEmptyGuid]
        [PrimaryKey]
        [CacheKey]
        public Guid Id { get; set; } = SecurityUtil.CreateSequentialGuid(DateTimeOffset.UtcNow, GuidStoredFormat.AsBinary);

        //TODO: 这里是按AsBinary来生成的，在不同的数据库需要不同的生成
    }
}
