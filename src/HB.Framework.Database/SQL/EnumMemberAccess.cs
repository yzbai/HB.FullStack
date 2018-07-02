using System;
using System.Reflection;

namespace HB.Framework.Database.SQL
{
    /// <summary>
    /// Wrapper of String & Enum
    /// 
    /// </summary>
    /// 

    public class EnumMemberAccess : PartialSqlString
    {
        public EnumMemberAccess(string text, Type enumType)
            : base(text)
        {
            if (!enumType.GetTypeInfo().IsEnum)
            {
                throw new ArgumentException("Type not valid", "enumType");
            }

            EnumType = enumType;
        }

        public Type EnumType { get; private set; }
    }
}
