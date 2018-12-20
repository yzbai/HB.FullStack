using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace HB.Framework.Common.Test
{
    public class SecurityHelperTest
    {
        private ITestOutputHelper _output;

        public SecurityHelperTest(ITestOutputHelper outputHelper)
        {
            _output = outputHelper;
            
        }

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
        [MemberData(nameof(get_randomNumbericStringLength))]
        public void CreateRandomNumbericStringTest(int length)
        {
            _output.WriteLine($"xxxxxxxxxxxxxxxxxxxxxxxxxxxxlength : {length}");

            string result = SecurityHelper.CreateRandomNumbericString(length).Trim();

            Assert.Equal(result.Length, length);
        }
    }
}
