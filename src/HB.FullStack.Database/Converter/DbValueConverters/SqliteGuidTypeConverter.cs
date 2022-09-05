

using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    internal class SqliteGuidTypeConverter : IDbValueConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "CHAR(36)";

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {
            return new Guid((string)dbValue);
        }

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            return ((Guid)typeValue).ToString();
        }
    }
}