/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;
using HB.FullStack.Common.PropertyTrackable;

namespace HB.FullStack.Database.DbModels
{
    public interface IDbModel : IModel
    {
        object? Id { get; set; }

        bool Deleted { get; set; }

        string? LastUser { get; set; }
    }

    public abstract class DbModel<TId> : ValidatableObject, IDbModel
    {
        [DbField(0)]
        [DbPrimaryKey]
        [CacheModelKey]
        [Required]
        public abstract TId Id { get; set; }

        [DbField(1)]
        public abstract bool Deleted { get; set; }

        [DbField(2)]
        public abstract string? LastUser { get; set; }

        public ModelKind GetKind() => ModelKind.Db;

        object? IDbModel.Id { get => Id; set => Id = (TId)value!; }
    }

    //[PropertyTrackableObject]
    //public abstract partial class ExpiredDbModel<TId> : DbModel<TId>, IExpired
    //{
    //    [TrackProperty]
    //    private long? _expiredAt;

    //    /// <summary>
    //    /// 改动时间，包括：
    //    /// 1. Update
    //    /// 2. Get from network
    //    /// </summary>
    //    //[TrackProperty]
    //    //private DateTimeOffset _lastTime = DateTimeOffset.UtcNow;
    //}
}