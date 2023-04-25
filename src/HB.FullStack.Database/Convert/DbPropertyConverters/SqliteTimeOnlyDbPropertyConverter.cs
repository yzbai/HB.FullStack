
using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    internal class SqliteTimeOnlyDbPropertyConverter : IDbPropertyConverter
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
            return TimeOnly.Parse(dbValue.ToString()!, Globals.Culture);
        }
    }
}