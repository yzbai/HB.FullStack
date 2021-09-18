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
        /// <summary>
        /// Test1Async
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [Fact]
        public async Task Test1Async()
        {
            FlackIdGen.Initialize(new Microsoft.Extensions.DependencyInjection.IdGenSettings { MachineId = 0 });
            ConcurrentDictionary<long, int> dict = new ConcurrentDictionary<long, int>();

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 100; ++i)
            {
                tasks.Add(GenerateIdAsync(dict));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var ordered = dict.OrderBy(kv => kv.Key);
        }

        /// <summary>
        /// GenerateIdAsync
        /// </summary>
        /// <param name="dict"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private static Task GenerateIdAsync(ConcurrentDictionary<long, int> dict)
        {
            Task task = new Task(() =>
            {
                int taskID = Task.CurrentId!.Value;
                for (int i = 0; i < 10; ++i)
                {
                    long id = StaticIdGen.GetId();
                    Debug.WriteLine($"{id}    {taskID}");
                    if (!dict.TryAdd(id, taskID))
                    {
#pragma warning disable CA2201 // Do not raise reserved exception types
                        throw new Exception("duplicated");
#pragma warning restore CA2201 // Do not raise reserved exception types
                    }
                }
            });

            task.Start();

            return task;

        }
    }
}
