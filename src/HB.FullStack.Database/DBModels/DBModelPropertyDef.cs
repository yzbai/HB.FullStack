﻿using System;
using System.Reflection;

using HB.FullStack.Common.Models;
using HB.FullStack.Database.Convert;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.DbModels
{
    /// <summary>
    /// 实体内属性定义（一个）
    /// </summary>
    public class DbModelPropertyDef : ModelPropertyDef
    {
        #region Common ModelPropertyDef

        public DbModelDef ModelDef { get; set; } = null!;

        public Type? NullableUnderlyingType { get; set; }

        public MethodInfo GetMethod { get; set; } = null!;

        public MethodInfo SetMethod { get; set; } = null!;

        #endregion

        public string DbReservedName { get; set; } = null!;

        public string DbParameterizedName { get; set; } = null!;

        public bool IsAutoIncrementPrimaryKey { get; set; }

        public bool IsIndexNeeded { get; set; }

        public bool IsPrimaryKey { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsUnique { get; set; }

        public bool IsNullable { get; set; }

        public bool IsLengthFixed { get; set; }

        public int? DbMaxLength { get; set; }

        public IDbPropertyConverter? TypeConverter { get; set; }

        public object? GetValueFrom(object model)
        {
            return GetMethod.Invoke(model, null);
        }

        public void SetValueTo(object model, object? value)
        {
            SetMethod.Invoke(model, new object?[] { value });
        }
    }
}