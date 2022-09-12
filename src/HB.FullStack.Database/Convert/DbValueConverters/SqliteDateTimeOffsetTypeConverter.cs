
using System;
using System.Data;
using System.Globalization;

using HB.FullStack.Common;

namespace HB.FullStack.Database.Convert
{
    internal class SqliteDateTimeOffsetTypeConverter : IDbValueConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "VARCHAR";

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            //Microsoft.Data.Sqlite会自动ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz", CultureInfo.InvariantCulture);
            return typeValue;
        }

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            return DateTimeOffset.Parse(dbValue.ToString()!, GlobalSettings.Culture);
        }
    }
}