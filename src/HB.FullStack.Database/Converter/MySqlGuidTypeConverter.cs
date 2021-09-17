#nullable enable

using System;
using System.Data;

namespace HB.FullStack.Database.Converter
{
    internal class MySqlGuidTypeConverter : ITypeConverter
    {
        public DbType DbType => DbType.Binary;

        public string Statement => "Binary(16)";

        public object DbValueToTypeValue(object dbValue, Type propertyType)
        {

            return new Guid((byte[])dbValue);
        }

        public object TypeValueToDbValue(object typeValue, Type propertyType)
        {
            return ((Guid)typeValue).ToByteArray();
        }
    }
}