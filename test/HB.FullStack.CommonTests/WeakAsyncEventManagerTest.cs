using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.FullStack.Common;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace HB.FullStack.CommonTests
{
    [TestClass]
    public class WeakAsyncEventManagerTest
    {
        private readonly WeakAsyncEventManager _weakAsyncEventManager = new WeakAsyncEventManager();


        public event AsyncEventHandler Updating
        {
            add => _weakAsyncEventManager.Add(value);
            remove => _weakAsyncEventManager.Remove(value);
        }

        [TestMethod]
        public async Task ConcurrenceTest()
        {
            List<int> orderList = new List<int>();

            Updating += async (sender, args) =>
            {
                orderList.Add(1);
                Console.WriteLine("Task 1 Start");

                await Task.Delay(1000);

                orderList.Add(1);
                Console.WriteLine("Task 1 Finish.");
            };

            Updating += async (sender, args) =>
            {
                orderList.Add(2);
                Console.WriteLine("Task 2 Start");

                await Task.Delay(80);

                orderList.Add(2);
                Console.WriteLine("Task 2 Finish.");
            };

            Updating += async (sender, args) =>
            {
                orderList.Add(3);
                Console.WriteLine("Task 3 Start");

                await Task.Delay(900);

                orderList.Add(3);
                Console.WriteLine("Task 3 Finish.");
            };

            Updating += async (sender, args) =>
            {
                orderList.Add(4);
                Console.WriteLine("Task 4 Start");

                await Task.Delay(300);

                orderList.Add(4);
                Console.WriteLine("Task 4 Finish.");
            };

            Updating += async (sender, args) =>
            {
                orderList.Add(5);
                Console.WriteLine("Task 5 Start");

                await Task.Delay(100);

                orderList.Add(5);
                Console.WriteLine("Task 5 Finish.");
            };

            _weakAsyncEventManager.RaiseEventAsync(nameof(Updating), this, EventArgs.Empty).Fire();

            await Task.Delay(1200);

            CollectionAssert.AreEqual(orderList, new int[] {1,2,3,4,5, 2, 5, 4, 3, 1});

        }
    }
}
