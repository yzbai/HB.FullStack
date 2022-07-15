﻿using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Cache.CacheModels;
using HB.FullStack.Common.IdGen;

namespace HB.FullStack.Database.DBModels
{
    public abstract class TimestampDBModel : DBModel, ICacheModel
    {
        //public int Version { get; set; } = -1;

        public string LastUser { get; set; } = string.Empty;

        /// <summary>
        /// 取代Version，实现行粒度。
        /// Version存在UserA将Version为1大老数据更改两次得到Version3，UserB将Version为2的数据更改一次变成Version3，都是version3，但经过路径不同，但系统认为相同。
        /// 就把Timestamp看作Version就行
        /// </summary>
        public long Timestamp { get; set; } = TimeUtil.UtcNowTicks;


        //public DateTimeOffset LastTime { get; set; } = TimeUtil.UtcNow;
    }




    public abstract class TimestampLongIdDBModel : TimestampDBModel, ILongIdModel
    {
        [DBModelProperty(0)]
        public abstract long Id { get; set; }
    }

    public abstract class TimestampAutoIncrementIdDBModel : TimestampLongIdDBModel, IAutoIncrementId
    {
        [AutoIncrementPrimaryKey]
        [DBModelProperty(0)]
        [CacheModelKey]
        public override long Id { get; set; } = -1;
    }

    public abstract class TimestampFlackIdDBModel : TimestampLongIdDBModel
    {
        [PrimaryKey]
        [DBModelProperty(0)]
        [CacheModelKey]
        [LongId2]
        public override long Id { get; set; } = StaticIdGen.GetId();
    }

    public abstract class TimestampGuidDBModel : TimestampDBModel, IGuidIdModel
    {
        [DBModelProperty(0)]
        [NoEmptyGuid]
        [PrimaryKey]
        [CacheModelKey]
        public Guid Id { get; set; } = SecurityUtil.CreateSequentialGuid(DateTimeOffset.UtcNow, GuidStoredFormat.AsBinary);

        //TODO: 这里是按AsBinary来生成的，在不同的数据库需要不同的生成
    }

}