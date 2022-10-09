
using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    internal class SqliteGuidDbPropertyConverter : IDbPropertyConverter
    {
        public DbType DbType => DbType.String;

        public string DbTypeStatement => "CHAR(36)";

        public object DbFieldValueToPropertyValue(object dbValue, Type propertyType)
        {
            return new Guid((string)dbValue);
        }

        public object PropertyValueToDbFieldValue(object typeValue, Type propertyType)
        {
            return ((Guid)typeValue).ToString();
        }
    }
}