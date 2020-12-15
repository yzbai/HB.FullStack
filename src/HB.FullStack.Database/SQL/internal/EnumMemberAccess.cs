using System;

using HB.FullStack.Database.Properties;

namespace HB.FullStack.Database.SQL
{
    /// <summary>
    /// Wrapper of String & Enum
    ///
    /// </summary>
    ///

    internal class EnumMemberAccess : PartialSqlString
    {
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