using System;
using System.Reflection;
using System.Data;

namespace HB.Framework.Database.Entity
{
    /// <summary>
    /// 实体内属性定义（一个）
    /// </summary>
    public class DatabaseEntityPropertyDef
    {
        #region 自身描述
        /// <summary>
        /// 所属实体名称
        /// </summary>
        public DatabaseEntityDef EntityDef { get; set; }
        /// <summary>
        /// 属性名称
        /// </summary>
        public string PropertyName { get; set; }
        /// <summary>
        /// 属性类型
        /// </summary>
        public Type PropertyType { get; set; }
        /// <summary>
        /// Get方法
        /// </summary>
        public MethodInfo GetMethod { get; set; }
        /// <summary>
        /// Set方法
        /// </summary>
        public MethodInfo SetMethod { get; set; }

        #endregion

        #region 数据库

        /// <summary>
        /// 是否是数据库表字段
        /// </summary>
        public bool IsTableProperty { get; set; } = false;
        /// <summary>
        /// 数据库引号化后的名称
        /// IsTableProperty为false时无意义
        /// </summary>
        public string DbReservedName { get; set; }
        /// <summary>
        /// 数据库参数化后的名称
        /// IsTableProperty为false时无意义
        /// </summary>
        public string DbParameterizedName { get; set; }
        /// <summary>
        /// 数据库中对应类型
        /// IsTableProperty为false时无意义
        /// </summary>
        public DbType DbFieldType { get; set; }
        /// <summary>
        /// 是否是主键
        /// IsTableProperty为false时无意义
        /// </summary>
        public bool IsPrimaryKey { get; set; } = false;
        /// <summary>
        /// 是否是外键
        /// IsTableProperty为false时无意义
        /// </summary>
        public bool IsForeignKey { get; set; } = false;
        /// <summary>
        /// 是否唯一值
        /// IsTableProperty为false时无意义
        /// </summary>
        public bool IsUnique { get; set; } = false;

        /// <summary>
        /// 是否可空
        /// IsTableProperty为false时无意义
        /// </summary>
        public bool IsNullable { get; set; } = true;

        /// <summary>
        /// 长度是否限定
        /// </summary>
        public bool IsLengthFixed { get; set; } = false;
        /// <summary>
        /// 数据库字段长度
        /// IsTableProperty为false时无意义
        /// </summary>
        public int? DbLength { get; set; }
        /// <summary>
        /// 数据库默认值
        /// IsTableProperty为false时无意义
        /// </summary>
        public string DbDefaultValue { get; set; }
        /// <summary>
        /// 数据库中备注、描述
        /// IsTableProperty为false时无意义
        /// </summary>
        public string DbDescription { get; set; }

        #endregion

        public DatabaseTypeConverter TypeConverter { get; set; }

        public DatabaseEntityPropertyDef() { }
        /// <summary>
        /// 获取值
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        public object GetValue(object domain)
        {
            return GetMethod.Invoke(domain, null);
        }
        ///// <summary>
        ///// 获取数据库值表达
        ///// </summary>
        ///// <param name="domain"></param>
        ///// <returns></returns>
        //public string GetDbValueStatement(object domain)
        //{
        //    object value = GetMethod.Invoke(domain, null);
        //    return DatabaseEngine.GetDbValueStatement(value);
        //}

        /// <summary>
        /// 赋值
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="value"></param>
        public void SetValue(object domain, object value)
        {
            SetMethod.Invoke(domain, new object[] { value });
        }
    }
}
