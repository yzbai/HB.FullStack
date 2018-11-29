using HB.Component.Resource.Sms;
using HB.Infrastructure.Aliyun.Test;
using Xunit;
using Xunit.Abstractions;

namespace HB.PresentFish.Tools
{
    public class SmsTest : IClassFixture<TestFixture>
    {
        private ISmsService _smsBiz;
        private TestFixture _fixture;
        private ITestOutputHelper _output;

        public SmsTest(ITestOutputHelper output, TestFixture testFixture)
        {
            _output = output;
            _fixture = testFixture;
            _smsBiz = _fixture.SmsService;
        }

        [Theory]
        [InlineData("15190208956")]
        [InlineData("18015323958")]
        public void SendSms(string mobile)
        {
            var result = _smsBiz.SendValidationCode(mobile, out string code).Result;

            _output.WriteLine(result.Message);

            Assert.True(result.Succeeded);
        }
    }
}
