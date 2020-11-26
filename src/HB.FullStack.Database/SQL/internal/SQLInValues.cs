using HB.FullStack.Database.Engine;
using System.Collections;
using System.Text;

namespace HB.FullStack.Database.SQL
{
    /// <summary>
    /// 工具类：输出 以逗号链接的值 字符串。（ex: value1,value2,value3）.
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

        public string ToSqlInString(IDatabaseEngine dbEngine)
        {
            if (Count == 0)
                return "NULL";

            return SqlJoin(_values, dbEngine);
        }

        public static string SqlJoin(IEnumerable values, IDatabaseEngine dbEngine)
        {
            StringBuilder sb = new StringBuilder();

            foreach (object value in values)
            {
                sb.Append(dbEngine.GetDbValueStatement(value, needQuoted: true));
                sb.Append(',');
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
