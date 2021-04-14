#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

namespace HB.FullStack.Database.Converter
{
    public class LongIdsTypeConverter : ITypeConverter
    {
        public DbType DbType => DbType.String;

        public string Statement => "VARCHAR";

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            string? str = dbValue.ToString();

            if (str.IsNotNullOrEmpty())
            {
                return str.Split(',').Select(i => Convert.ToInt64(i, CultureInfo.InvariantCulture));
            }

            return new List<long>();
        }

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            if (typeValue is IEnumerable<long> ids)
            {
                return ids.ToJoinedString(",");
            }

            return "";
        }
    }
}