using Microsoft.VisualStudio.TestTools.UnitTesting;
using HB.Infrastructure.Tencent;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HB.Infrastructure.Tencent.TCaptha;

namespace HB.Infrastructure.Tencent.Tests
{
    [TestClass()]
    public class TCapthaClientTests
    {
        [TestMethod()]
        public void TCapthaResultSerializeTest()
        {
            //262
            string json2 = "{\\\"appid\\\":\\\"2029147713\\\",\\\"ret\\\":0,\\\"ticket\\\":\\\"t03cBcP6ltlugGB2Qt6YybQ0yXOwx3O_mqg6oSOMC85cVCvpuF7LR_aGg1dyU6G28lUM6bpQntv5qbNyIox9ZWUQTn08KsLHKYuBb3bgMYHx7JYj_PJLL71pKlap7usAN088mxqVCYCN6irNUFlrECeMSF5TgvnqH3UN9nsnIGHttGYjGWqoKz-Pw**\\\",\\\"randstr\\\":\\\"@w2o\\\"}";
            string json = "{\"appid\":\"2029147713\",\"ret\":0,\"ticket\":\"t03wWvosvqCcRJ4wHfv_e7zki3hckDc4m2aY1ei-1-qiHg5drBDt6K0cyBgsEAJAubUrFGCpEml7__uC_3A2EUc9HYJUouKin5_cKiqT4a6ucWwALpCtV1TMo_3VOiyo7gAvt3mnT1AKkogFs7Eo0LibOH5NV8_-nCLcnF7s-IO70wMJP4CB36U9w**\",\"randstr\":\"@V7d\"}";
            int len = Encoding.UTF8.GetBytes(json).Length;


            TCaptchaResult? result = SerializeUtil.FromJson<TCaptchaResult>(json2);

            Assert.IsTrue(result != null);
        }
    }
}