using HB.FullStack.Database.Properties;
using System;

namespace HB.FullStack.Database.SQL
{
    /// <summary>
    /// Wrapper of String & Enum
    /// 
    /// </summary>
    /// 

    internal class EnumMemberAccess : PartialSqlString
    {
        /// <exception cref="System.ArgumentException"></exception>
        public EnumMemberAccess(string text, Type enumType)
            : base(text)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException(Resources.TypeNotValidErrorMessage, nameof(enumType));
            }

            EnumType = enumType;
        }

        public Type EnumType { get; private set; }
    }
}
