using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HB.Component.Resource.Sms;
using HB.Framework.Common;
using HB.Framework.EventBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace WebDemo.Controllers
{

    public class TestEntity
    {
        public string Name { get; set; }

        public string Message { get; set; }
    }

    [Route("api/[controller]")]
    public class ValuesController : Controller
    {
        private readonly ILogger _logger;

        private readonly IEventBus _eventBus;

        //private static int runTimes = 0;

        private readonly ISmsService _smsBiz; 

        public ValuesController(ILogger<ValuesController> logger, IEventBus eventBus, ISmsService smsBiz)
        {
            _logger = logger;
            _eventBus = eventBus;
            _smsBiz = smsBiz;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            

            return new string[] { "value1", "value4346464" };
        }

        private static void TaskRetryTest()
        {
            //int mm = 0;

            //var task = TaskRetry.Retry<int>(2, () => Task.Run<int>(() =>
            //{

            //    runTimes++;

            //    if (runTimes < 4)
            //    {
            //        int a = 1 / mm;
            //        Console.Write(a);
            //    }

            //    return new Random().Next(1000);
            //}),
            //(ret, ex) =>
            //{
            //    _logger.LogInformation("Runtimes exception : " + runTimes);
            //    _logger.LogCritical(ex.Message);
            //});

            //task.ContinueWith(t =>
            //{
            //    _logger.LogCritical("The final value status true / false : " + t.IsFaulted);
            //});

            //TestEntity entity = new TestEntity {
            //    Name = "Haha,You",
            //    Message = "Hello , There"
            //};

            //_eventBus.Publish("WebDemo.Test", DataConverter.ToJson(entity));
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
