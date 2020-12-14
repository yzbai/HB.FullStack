using System.Collections;
using System.Text;

using HB.FullStack.Database.Converter;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    /// <summary>
    /// 工具类：输出 以逗号链接的值 字符串。（ex: value1,value2,value3）.
    /// 目前只支持没有TypeConverter（全局或者属性特有）的属性
    /// </summary>
    internal class SQLInValues
    {
        private readonly IEnumerable _values;

        public int Count { get; private set; }

        public SQLInValues(IEnumerable values)
        {
            _values = values;

            if (values != null)
            {
                foreach (object _ in values)
                {
                    ++Count;
                }
            }
        }

        public string ToSqlInString(DatabaseEngineType engineType)
        {
            if (Count == 0)
                return "NULL";

            return SqlJoin(_values, engineType);
        }

        public static string SqlJoin(IEnumerable values, DatabaseEngineType engineType)
        {
            StringBuilder sb = new StringBuilder();

            foreach (object value in values)
            {
                sb.Append(TypeConvert.TypeValueToDbValueStatement(value, quotedIfNeed: true, engineType));
                sb.Append(',');
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}