using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    /// <summary>
    /// 统一的放置 TypeValue与String之间的转换
    /// </summary>
    public static class TypeStringConverter
    {
        public static string ConvertToString(object? value)
        {
            return value == null ? string.Empty : value.ToString()!;
        }

    }
}
