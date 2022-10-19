
using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbPropertyConverter
    {
        DbType DbType { get; }

        string DbTypeStatement { get; }

        /// <summary>
        /// 将CLR中的值转为数据库存储的值
        /// </summary>
        object PropertyValueToDbFieldValue(object propertyValue, Type propertyType);

        /// <summary>
        /// 将数据库存储值转为CLR值
        /// </summary>
        object DbFieldValueToPropertyValue(object dbFieldValue, Type propertyType);
    }
}