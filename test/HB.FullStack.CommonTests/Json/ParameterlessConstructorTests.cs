using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.FullStack.CommonTests.Json
{
    [TestClass]
    public class ParameterlessConstructorTests
    {
        /// <summary>
        /// System.Text.Json 不支持没有包含无参构造函数的类
        /// </summary>
        [TestMethod]
        public void Json_Parameterless_Test()
        {
            SimpleCls simpleCls = new SimpleCls("xx", "tt", 12);

            string json = SerializeUtil.ToJson(simpleCls);
            var fromJson = SerializeUtil.FromJson<SimpleCls>(json);
            string json2 = SerializeUtil.ToJson(fromJson!);

            Assert.AreEqual(json, json2);
        }

        /// <summary>
        /// IConfiguration.Bind不支持没有包含无参构造函数的类
        /// </summary>
        [TestMethod]
        public void Configuration_Bind_Parameterless_Test()
        {
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("app.json").Build();

            SimpleAppOptions simpleAppOptions = new SimpleAppOptions();

            configuration.GetSection("SimpleAppOptions").Bind(simpleAppOptions);
        }

        [TestMethod]
        public void Options_Parameterless_Test()
        {
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("app.json").Build();

            ServiceCollection services = new ServiceCollection();

            services.Configure<SimpleAppOptions>(configuration.GetSection("SimpleAppOptions"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            IOptions<SimpleAppOptions> options = serviceProvider.GetRequiredService<IOptions<SimpleAppOptions>>();
            _ = options.Value;
        }
    }
}
