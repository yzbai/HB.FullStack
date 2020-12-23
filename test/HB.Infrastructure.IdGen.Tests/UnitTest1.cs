using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HB.FullStack.Common.IdGen;
using Xunit;

namespace HB.Infrastructure.IdGen.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1Async()
        {
            IdGenDistributedId.Initialize(0);
            ConcurrentDictionary<long, int> dict = new ConcurrentDictionary<long, int>();

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 100; ++i)
            {
                tasks.Add(GenerateIdAsync(dict));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var ordered = dict.OrderBy(kv => kv.Key);
        }

        private Task GenerateIdAsync(ConcurrentDictionary<long, int> dict)
        {
            Task task = new Task(() =>
            {
                int taskID = Task.CurrentId.Value;
                for (int i = 0; i < 10; ++i)
                {
                    long id = IDistributedIdGen.IdGen.GetId();
                    Debug.WriteLine($"{id}    {taskID}");
                    if (!dict.TryAdd(id, taskID))
                    {
                        throw new Exception("duplicated");
                    }
                }
            });

            task.Start();

            return task;

        }
    }
}
