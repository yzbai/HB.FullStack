using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using HB.FullStack.Common.IdGen;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HB.Infrastructure.IdGen.Tests
{
    [TestClass]
    public class IdGenTests
    {
        [TestMethod]
        public async Task ConcurrencyTestAsync()
        {
            FlackIdGen.Initialize(new IdGenSettings { MachineId = 0 });
            ConcurrentDictionary<long, int> dict = new ConcurrentDictionary<long, int>();

            List<Task> tasks = new List<Task>();

            for (int i = 0; i < 100; ++i)
            {
                tasks.Add(GenerateIdAsync(dict));
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);

            var ordered = dict.OrderBy(kv => kv.Key);

            CollectionAssert.AllItemsAreUnique(ordered.Select(kv => kv.Key).ToArray());
        }

        private static Task GenerateIdAsync(ConcurrentDictionary<long, int> dict)
        {
            Task task = new Task(() =>
            {
                int taskID = Task.CurrentId!.Value;
                for (int i = 0; i < 10; ++i)
                {
                    long id = StaticIdGen.GetId();

                    Console.WriteLine($"{id}    {taskID}");

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