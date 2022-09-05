using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;
using HB.FullStack.Common.IdGen;

namespace HB.FullStack.Database.DbModels
{
    /// <summary>
    /// 适合不怎么冲突的Model，没有行乐观锁
    /// </summary>
    public abstract class TimelessDbModel : DbModel
    {

    }

    public abstract class TimelessLongIdDbModel : TimelessDbModel, ILongId
    {
        [DBModelProperty(0)]
        public abstract long Id { get; set; }
    }

    public abstract class TimelessAutoIncrementIdDbModel : TimelessLongIdDbModel, IAutoIncrementId
    {
        [AutoIncrementPrimaryKey]
        [DBModelProperty(0)]
        [CacheModelKey]
        public override long Id { get; set; } = -1;
    }

    public abstract class TimelessFlackIdDbModel : TimelessLongIdDbModel
    {
        [PrimaryKey]
        [DBModelProperty(0)]
        [CacheModelKey]
        [LongId2]
        public override long Id { get; set; } = StaticIdGen.GetId();
    }

    public abstract class TimelessGuidDbModel : TimelessDbModel, IGuidId
    {
        [DBModelProperty(0)]
        [NoEmptyGuid]
        [PrimaryKey]
        [CacheModelKey]
        public Guid Id { get; set; } = SecurityUtil.CreateSequentialGuid(DateTimeOffset.UtcNow, GuidStoredFormat.AsBinary);

        //TODO: 这里是按AsBinary来生成的，在不同的数据库需要不同的生成
    }
}
