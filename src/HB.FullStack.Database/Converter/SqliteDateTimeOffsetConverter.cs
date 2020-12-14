#nullable enable

using System;
using System.Data;
using System.Globalization;

namespace HB.FullStack.Database.Converter
{
    internal class SqliteDateTimeOffsetConverter : ITypeConverter
    {
        public DbType DbType => DbType.String;

        public string Statement => "VARCHAR";

        public object TypeValueToDbValue(object? typeValue, Type propertyType)
        {
            if (typeValue == null)
            {
                return DBNull.Value;
            }

            //Microsoft.Data.Sqlite会自动ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz", CultureInfo.InvariantCulture);
            return typeValue;
        }

        public object? DbValueToTypeValue(object dbValue, Type dbValueType, Type propertyType)
        {
            if (dbValueType == typeof(DBNull))
            {
                return default(DateTimeOffset);
            }

            return DateTimeOffset.Parse(dbValue.ToString(), CultureInfo.InvariantCulture);
        }
    }
}