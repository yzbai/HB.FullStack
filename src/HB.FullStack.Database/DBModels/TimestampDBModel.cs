using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Database.DbModels
{
    /// <summary>
    /// 使用timestamp 或者新旧值比较 实现版本冲突检测
    /// </summary>
    //public abstract class TimestampDbModel<T> : DbModel2<T>, ITimestamp
    //{
    //    /// <summary>
    //    /// 取代Version，实现行粒度。
    //    /// Version存在UserA将Version为1大老数据更改两次得到Version3，UserB将Version为2的数据更改一次变成Version3，都是version3，但经过路径不同，但系统认为相同。
    //    /// 就把Timestamp看作Version就行
    //    /// </summary>
    //    //private long _timestamp = TimeUtil.Timestamp;

    //    //[Range(638000651894004864, long.MaxValue)]
    //    //public long Timestamp
    //    //{
    //    //    get => _timestamp;
    //    //    internal set
    //    //    {
    //    //        if (this is IPropertyTrackableObject trackableObject)
    //    //        {
    //    //            trackableObject.Track(nameof(Timestamp), _timestamp, value);
    //    //        }

    //    //        _timestamp = value;
    //    //    }
    //    //}

    //    [DbField(3)]
    //    [AddtionalProperty]
    //    [Range(638000651894004864, long.MaxValue)]
    //    public abstract long Timestamp { get; set; }    
    //}

}