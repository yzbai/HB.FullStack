using HB.Compnent.Common.Sms;
using HB.Infrastructure.Aliyun.Test;
using Xunit;
using Xunit.Abstractions;

namespace HB.PresentFish.Tools
{
    public class SmsTest : IClassFixture<TestFixture>
    {
        private ISmsBiz _smsBiz;
        private TestFixture _fixture;
        private ITestOutputHelper _output;

        public SmsTest(ITestOutputHelper output, TestFixture testFixture)
        {
            _output = output;
            _fixture = testFixture;
            _smsBiz = _fixture.GetSmsBiz();
        }

        [Theory]
        [InlineData("15190208956")]
        [InlineData("18015323958")]
        public void SendSms(string mobile)
        {
            var result = _smsBiz.SendIdentityValidationCode(mobile, out string code).Result;

            _output.WriteLine(result.Message);

            Assert.True(result.Succeeded);
        }
    }
}
