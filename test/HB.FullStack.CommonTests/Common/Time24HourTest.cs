using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HB.FullStack.Common;

using Xunit;

namespace HB.FullStack.Tests.Common
{
    public class Time24HourTest
    {
        [Fact]
        public void Test_Time24Hour()
        {
            string time1Str = "am0:1";
            string time2Str = "pm12:00";
            string time3Str = "am11:59";
            string time4Str = "pm11:59";
            string time5Str = "pm12:01";

            Time24Hour time1 = new Time24Hour(time1Str);
            Time24Hour time2 = new Time24Hour(time2Str);
            Time24Hour time3 = new Time24Hour(time3Str);
            Time24Hour time4 = new Time24Hour(time4Str);
            Time24Hour time5 = new Time24Hour(time5Str);

            Assert.True(time1 == new Time24Hour(0, 1) && time1.IsAm);
            Assert.True(time2 == new Time24Hour(12, 0) && !time2.IsAm);
            Assert.True(time3 == new Time24Hour(11, 59) && time3.IsAm);
            Assert.True(time4 == new Time24Hour(23, 59) && !time4.IsAm);
            Assert.True(time5 == new Time24Hour(12, 1) && !time5.IsAm);

            
        }
    }
}
