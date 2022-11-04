
using System;
using System.Data;
using System.Globalization;

using HB.FullStack.Common;

namespace HB.FullStack.Database.Convert
{
    internal class SqliteDateTimeOffsetDbPropertyConverter : IDbPropertyConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "VARCHAR";

        public object PropertyValueToDbFieldValue(object typeValue, Type propertyType)
        {
            //Microsoft.Data.Sqlite会自动ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz", CultureInfo.InvariantCulture);
            return typeValue;
        }

        public object DbFieldValueToPropertyValue(object dbValue, Type propertyType)
        {
            return DateTimeOffset.Parse(dbValue.ToString()!, Globals.Culture);
        }
    }
}