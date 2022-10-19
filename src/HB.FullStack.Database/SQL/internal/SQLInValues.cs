using System.Collections;
using System.Text;

using HB.FullStack.Database.Convert;
using HB.FullStack.Database.Engine;

namespace HB.FullStack.Database.SQL
{
    /// <summary>
    /// 工具类：输出 以逗号链接的值 字符串。（ex: value1,value2,value3）.
    /// 目前只支持没有TypeConverter（全局或者属性特有）的属性
    /// </summary>
    internal class SQLInValues
    {
        public IEnumerable Values { get; private set; }

        public int Count { get; private set; }

        public SQLInValues(IEnumerable values)
        {
            Values = values;

            if (values != null)
            {
                foreach (object _ in values)
                {
                    ++Count;
                }
            }
        }
    }
}