#nullable enable

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace HB.FullStack.Database.Converter
{
    public class GuidListTypeConverter : ITypeConverter
    {
        public DbType DbType => DbType.String;

        public string Statement => "VARCHAR";

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            string? str = dbValue.ToString();

            if (str.IsNotNullOrEmpty())
            {
                return str.Split(',').Select(i => new Guid(i)).ToList();
            }

            return new List<Guid>();
        }

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            if (typeValue is IEnumerable<Guid> guids)
            {
                return guids.ToJoinedString(",");
            }

            throw DatabaseExceptions.TypeConverterError("使用LongIdListTypeConverter的，必须可赋值为IEnumerable<Guid>", propertyType.FullName);
        }
    }
}