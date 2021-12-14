using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HB.FullStack.CommonTests
{
    [TestClass]
    public class TempTests
    {
        /// <summary>
        /// TimeSpan的格式要求：小时，分钟，秒必须是两位数，且不能超过限制
        /// </summary>
        [TestMethod]
        public void TestTimeSpan()
        {
            TimeSpan timeSpan = TimeSpan.FromDays(3);
            TimeSpan newTimeSpan = timeSpan.Add(TimeSpan.FromHours(25).Add(TimeSpan.FromMinutes(89)));
            var json = SerializeUtil.ToJson(newTimeSpan);
            TimeSpan rt = SerializeUtil.FromJson<TimeSpan>(json);

            Console.WriteLine(json);
            Assert.AreEqual(newTimeSpan, rt);

            TimeSpan rt2 = SerializeUtil.FromJson<TimeSpan>("\"4.23:59:59\"");

            Console.WriteLine(rt2);
            Console.WriteLine(SerializeUtil.ToJson(rt2));
        }

        [TestMethod]
        public void TestSimpleDateSerializable()
        {
            SimpleDate simpleDate = new SimpleDate { Day = 1, Month = 1, IsMonthLeap = false, IsNongli = true, Year = 2020 };

            string json = SerializeUtil.ToJson(simpleDate);

            Console.WriteLine(json);

            SimpleDate simpleDate1 = SerializeUtil.FromJson<SimpleDate>(json);

            string json2 = SerializeUtil.ToJson(simpleDate1);

            Console.WriteLine(json2);

            Assert.AreEqual(json, json2);
        }
    }
}
