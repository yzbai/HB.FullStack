#nullable enable

using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    public interface ITypeConverter
    {
        DbType DbType { get; }

        string Statement { get; }

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