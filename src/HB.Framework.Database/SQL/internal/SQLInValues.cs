using HB.Framework.Database.Engine;
using System.Collections;
using System.Text;

namespace HB.Framework.Database.SQL
{
    /// <summary>
    /// 工具类：输出 以逗号链接的值 字符串。（ex: value1,value2,value3）.
    /// </summary>
    internal class SQLInValues
    {
        private readonly IEnumerable values;

        public int Count { get; private set; }

        public SQLInValues(IEnumerable values)
        {
            this.values = values;

            if (values != null)
            {
                foreach (var value in values)
                {
                    ++Count;
                }
            }
        }

        public string ToSqlInString(IDatabaseEngine dbEngine)
        {
            if (Count == 0)
                return "NULL";

            return SqlJoin(values, dbEngine);
        }

        public static string SqlJoin(IEnumerable values, IDatabaseEngine dbEngine)
        {
            var sb = new StringBuilder();

            foreach (var value in values)
            {
                sb.Append(dbEngine.GetDbValueStatement(value, needQuoted: true));
                sb.Append(",");
            }

            sb.Remove(sb.Length - 1, 1);

            return sb.ToString();
        }
    }
}
