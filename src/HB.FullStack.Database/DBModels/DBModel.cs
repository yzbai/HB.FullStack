/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Database.DbModels
{
    public abstract class BaseDbModel : ValidatableObject, IModel
    {
        public ModelKind GetKind() => ModelKind.Db;

        /// <summary>
        /// 不是真正的删除，而是用Deleted=1表示删除。
        /// </summary>
        [DbField(1)]
        public abstract bool Deleted { get; set; }

        [DbField(2)]
        public abstract string LastUser { get; set; }
    }

    public abstract class DbModel2<TId> : BaseDbModel
    {
        [DbField(0)]
        [DbPrimaryKey]
        [CacheModelKey]
        public abstract TId Id { get; set; }
    }
}