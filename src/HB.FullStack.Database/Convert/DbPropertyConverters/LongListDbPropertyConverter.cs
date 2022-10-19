using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace HB.FullStack.Database.Convert
{
    /// <summary>
    /// 将IEnumerable{long}转换为varchar
    /// </summary>
    public class LongListDbPropertyConverter : IDbPropertyConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "VARCHAR";

        public object DbFieldValueToPropertyValue(object dbValue, Type propertyType)
        {
            string? str = dbValue.ToString();

            if (str.IsNotNullOrEmpty())
            {
                return str.Split(',').Select(i => System.Convert.ToInt64(i, CultureInfo.InvariantCulture)).ToList();
            }

            return new List<long>();
        }

        public object PropertyValueToDbFieldValue(object typeValue, Type propertyType)
        {
            if (typeValue is IEnumerable<long> ids)
            {
                return ids.ToJoinedString(",");
            }

            throw DatabaseExceptions.TypeConverterError("使用LongIdListTypeConverter的，必须可赋值为IEnumerable<long>", propertyType.FullName);
        }
    }
}