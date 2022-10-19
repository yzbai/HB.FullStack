
using System;
using System.Data;

namespace HB.FullStack.Database.Convert
{
    internal class MySqlGuidDbPropertyConverter : IDbPropertyConverter
    {
        public DbType DbType => DbType.Binary;

        public string DbTypeStatement => "Binary(16)";

        public object DbFieldValueToPropertyValue(object dbValue, Type propertyType)
        {

            return new Guid((byte[])dbValue);
        }

        public object PropertyValueToDbFieldValue(object typeValue, Type propertyType)
        {
            return ((Guid)typeValue).ToByteArray();
        }
    }
}