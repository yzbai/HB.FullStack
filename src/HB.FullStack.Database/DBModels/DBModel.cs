﻿using System;
using System.ComponentModel.DataAnnotations;

using HB.FullStack.Common;
using HB.FullStack.Common.Cache.CacheModels;
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

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1040:Avoid empty interfaces", Justification = "<Pending>")]
    public interface IAutoIncrementId
    {

    }
}