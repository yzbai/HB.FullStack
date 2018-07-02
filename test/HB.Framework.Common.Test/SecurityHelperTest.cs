using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;

namespace HB.Framework.Common.Test
{
    public class SecurityHelperTest
    {
        public static IEnumerable<object[]> get_randomNumbericStringLength()
        {
            List<object[]> lst = new List<object[]>();
            Random random = new Random();

            for (int i = 0; i < 100; i++)
            {
                lst.Add(new object[] { random.Next(3, 9) });
            }

            return lst;
        }

        [Theory]
        [MemberData("get_randomNumbericStringLength")]
        public void CreateRandomNumbericStringTest(int length)
        {
            string result = SecurityHelper.CreateRandomNumbericString(length).Trim();

            Assert.Equal(result.Length, length);
        }
    }
}
