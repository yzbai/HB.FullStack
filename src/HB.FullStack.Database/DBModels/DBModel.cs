using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.IdGen;

namespace HB.FullStack.Database.DBModels
{
    public abstract class DBModel : Model
    {
        /// <summary>
        /// 不是真正的删除，而是用Deleted=1表示删除。
        /// </summary>
        public bool Deleted { get; /*internal*/ set; }
    }

    public abstract class TimelessDBModel : DBModel
    {

    }

    public abstract class TimelessLongIdDBModel : TimelessDBModel, ILongId
    {
        [DBModelProperty(0)]
        public abstract long Id { get; set; }
    }

    public abstract class TimelessAutoIncrementIdDBModel : TimelessLongIdDBModel, IAutoIncrementId
    {
        [AutoIncrementPrimaryKey]
        [DBModelProperty(0)]
        [CacheModelKey]
        public override long Id { get; set; } = -1;
    }

    public abstract class TimelessFlackIdDBModel : TimelessLongIdDBModel
    {
        [PrimaryKey]
        [DBModelProperty(0)]
        [CacheModelKey]
        [LongId2]
        public override long Id { get; set; } = StaticIdGen.GetId();
    }

    public abstract class TimelessGuidDBModel : TimelessDBModel, IGuidId
    {
        [DBModelProperty(0)]
        [NoEmptyGuid]
        [PrimaryKey]
        [CacheModelKey]
        public Guid Id { get; set; } = SecurityUtil.CreateSequentialGuid(DateTimeOffset.UtcNow, GuidStoredFormat.AsBinary);

        //TODO: 这里是按AsBinary来生成的，在不同的数据库需要不同的生成
    }
}