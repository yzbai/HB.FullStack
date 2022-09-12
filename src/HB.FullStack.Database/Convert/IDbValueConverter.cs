
using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    /// <summary>
    /// 
    /// </summary>
    public interface IDbValueConverter
    {
        DbType DbType { get; }

        string DbTypeStatement { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="typeValue">已经确保不为null</param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        object TypeValueToDbValue(object typeValue, Type propertyType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dbValue">确保不为null</param>
        /// <param name="propertyType"></param>
        /// <returns></returns>
        object DbValueToTypeValue(object dbValue, Type propertyType);
    }
}