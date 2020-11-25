using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HB.Framework.Common;
using Xunit;
using Xunit.Abstractions;

namespace HB.Framework.CommonTests
{
    public class WeakAsyncEventManagerTest
    {
        private WeakAsyncEventManager _weakAsyncEventManager = new WeakAsyncEventManager();
        private ITestOutputHelper _outputHelper;

        public WeakAsyncEventManagerTest(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }


        public event AsyncEventHandler Updating
        {
            add => _weakAsyncEventManager.Add(value);
            remove => _weakAsyncEventManager.Remove(value);
        }

        [Fact]
        public void Test_Concurrence()
        {
            Updating += async (sender, args) =>
            {
                await Task.Delay(10000);

                _outputHelper.WriteLine("Task 1 Finish.");
            };

            Updating += async (sender, args) =>
            {
                await Task.Delay(800);

                _outputHelper.WriteLine("Task 2 Finish.");
            };

            Updating += async (sender, args) =>
            {
                await Task.Delay(9000);

                _outputHelper.WriteLine("Task 3 Finish.");
            };

            Updating += async (sender, args) =>
            {
                await Task.Delay(3000);

                _outputHelper.WriteLine("Task 4 Finish.");
            };

            Updating += async (sender, args) =>
            {
                await Task.Delay(1000);

                _outputHelper.WriteLine("Task 5 Finish.");
            };

            _weakAsyncEventManager.RaiseEventAsync(nameof(Updating), this, EventArgs.Empty).Fire();

            Thread.Sleep(11000);
        }
    }
}
