﻿/*
 * Author：Yuzhao Bai
 * Email: yzbai@brlite.com
 * Github: github.com/yzbai
 * The code of this file and others in HB.FullStack.* are licensed under MIT LICENSE.
 */

using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Models;

namespace HB.FullStack.Database.DbModels
{
    public interface IDbModel : IModel
    {
        /// <summary>
        /// 不是真正的删除，而是用Deleted=1表示删除。
        /// </summary>
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

        public abstract bool Deleted { get; set; }

        public abstract string? LastUser { get; set; }

        public ModelKind GetKind() => ModelKind.Db;
    }
}