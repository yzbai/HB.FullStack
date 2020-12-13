using HB.FullStack.Common.Entities;

using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace HB.FullStack.Database.Entities
{
    /// <summary>
    /// 实体内属性定义（一个）
    /// </summary>
    internal class DatabaseEntityPropertyDef
    {
        public DatabaseEntityDef EntityDef { get; set; } = null!;

        public string Name { get; set; } = null!;

        public Type Type { get; set; } = null!;

        public Type? NullableUnderlyingType { get; set; }

        public MethodInfo GetMethod { get; set; } = null!;

        public MethodInfo SetMethod { get; set; } = null!;

        public string DbReservedName { get; set; } = null!;

        public string DbParameterizedName { get; set; } = null!;

        public bool IsAutoIncrementPrimaryKey { get; set; }

        public bool IsForeignKey { get; set; }

        public bool IsUnique { get; set; }

        public bool IsNullable { get; set; }

        public bool IsLengthFixed { get; set; }

        public int? DbMaxLength { get; set; }

        public TypeConverter? TypeConverter { get; set; }

        public object? GetValueFrom(object entity)
        {
            return GetMethod.Invoke(entity, null);
        }

        public void SetValueTo(object entity, object? value)
        {
            SetMethod.Invoke(entity, new object?[] { value });
        }
    }
}
