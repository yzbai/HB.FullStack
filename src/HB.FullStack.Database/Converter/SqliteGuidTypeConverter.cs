#nullable enable

using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    internal class SqliteGuidTypeConverter : ITypeConverter
    {
        public DbType DbType => DbType.String;

        public string Statement => "CHAR(36)";

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