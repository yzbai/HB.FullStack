

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Threading;

namespace System.Threading.Tasks
{
    public static class TaskUtil
    {
        /// <summary>
        /// 使用MySqlConnector的数据库操作，不能使用并行
        /// 详情请见 https://mysqlconnector.net/troubleshooting/connection-reuse/
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="task1"></param>
        /// <param name="task2"></param>
        /// <param name="convertor1"></param>
        /// <param name="convertor2"></param>
        /// <param name="continueOnCapturedContext"></param>
        /// <returns></returns>
        public static async Task<IEnumerable<TResult>> ConcurrenceAsync<TResult, T1, T2>(
           Func<Task<IEnumerable<T1>>> task1Func,
           Func<Task<IEnumerable<T2>>> task2Func,
           Func<IEnumerable<T1>, IEnumerable<TResult>> convertor1,
           Func<IEnumerable<T2>, IEnumerable<TResult>> convertor2,
           bool continueOnCapturedContext = false)
        {
            IList<TResult> results = new List<TResult>();
            Task<IEnumerable<T1>> task1 = task1Func();
            Task<IEnumerable<T2>> task2 = task2Func();

            IList<Task> tasks = new List<Task> { task1, task2 };

            //using JoinableTaskContext joinableTaskContext = new JoinableTaskContext();
            //JoinableTaskFactory joinableTaskFactory = new JoinableTaskFactory(joinableTaskContext);

            while (tasks.Any())
            {
                Task finished = await Task.WhenAny(tasks).ConfigureAwait(continueOnCapturedContext);
                tasks.Remove(finished);

                if (finished == task1)
                {
                    //#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                    //                    JoinableTask<IEnumerable<T1>> t1s = joinableTaskFactory.RunAsync(() => task1);
                    //#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

                    IEnumerable<T1> t1s = await task1.ConfigureAwait(false); //await task1.ConfigureAwait(false);

                    results.AddRange(convertor1(t1s));
                }

                if (finished == task2)
                {
                    //#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                    //                    JoinableTask<IEnumerable<T2>> t2s = joinableTaskFactory.RunAsync(() => task2);
                    //#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

                    IEnumerable<T2> t2s = await task2.ConfigureAwait(false);

                    results.AddRange(convertor2(t2s));
                }
            }

            return results;
        }

        /// <summary>
        /// 使用MySqlConnector的数据库操作，不能使用并行
        /// 详情请见 https://mysqlconnector.net/troubleshooting/connection-reuse/
        /// </summary>
        public static async Task<IEnumerable<TResult>> ConcurrenceAsync<TResult, T1, T2, T3>(
           Func<Task<IEnumerable<T1>>> task1Func,
           Func<Task<IEnumerable<T2>>> task2Func,
           Func<Task<IEnumerable<T3>>> task3Func,
           Func<IEnumerable<T1>, IEnumerable<TResult>> convertor1,
           Func<IEnumerable<T2>, IEnumerable<TResult>> convertor2,
           Func<IEnumerable<T3>, IEnumerable<TResult>> convertor3,
           bool continueOnCapturedContext = false)
        {
            IList<TResult> results = new List<TResult>();

            Task<IEnumerable<T1>> task1 = task1Func();
            Task<IEnumerable<T2>> task2 = task2Func();
            Task<IEnumerable<T3>> task3 = task3Func();

            IList<Task> tasks = new List<Task> { task1, task2, task3 };

            //using JoinableTaskContext joinableTaskContext = new JoinableTaskContext();
            //JoinableTaskFactory joinableTaskFactory = new JoinableTaskFactory(joinableTaskContext);

            while (tasks.Any())
            {
                Task finished = await Task.WhenAny(tasks).ConfigureAwait(continueOnCapturedContext);
                tasks.Remove(finished);

                if (finished == task1)
                {
                    //#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                    //                    JoinableTask<IEnumerable<T1>> t1s = joinableTaskFactory.RunAsync(() => task1);
                    //#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

                    IEnumerable<T1> t1s = await task1.ConfigureAwait(continueOnCapturedContext);

                    results.AddRange(convertor1(t1s));
                }

                if (finished == task2)
                {
                    //#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                    //                    JoinableTask<IEnumerable<T2>> t2s = joinableTaskFactory.RunAsync(() => task2);
                    //#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

                    IEnumerable<T2> t2s = await task2.ConfigureAwait(continueOnCapturedContext);

                    results.AddRange(convertor2(t2s));
                }

                if (finished == task3)
                {
                    //#pragma warning disable VSTHRD003 // Avoid awaiting foreign Tasks
                    //                    JoinableTask<IEnumerable<T3>> t3s = joinableTaskFactory.RunAsync(() => task3);
                    //#pragma warning restore VSTHRD003 // Avoid awaiting foreign Tasks

                    IEnumerable<T3> t3s = await task3.ConfigureAwait(continueOnCapturedContext);

                    results.AddRange(convertor3(t3s));
                }
            }

            return results;
        }

        /// <summary>
        /// 使用MySqlConnector的数据库操作，不能使用并行
        /// 详情请见 https://mysqlconnector.net/troubleshooting/connection-reuse/
        /// </summary>
        public static Task ConcurrenceAsync(IEnumerable<Task> tasks)
        {
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// 使用MySqlConnector的数据库操作，不能使用并行
        /// 详情请见 https://mysqlconnector.net/troubleshooting/connection-reuse/
        /// </summary>
        public static Task<TResult[]> ConcurrenceAsync<TResult>(IEnumerable<Task<TResult>> tasks)
        {
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// 使用MySqlConnector的数据库操作，不能使用并行
        /// 详情请见 https://mysqlconnector.net/troubleshooting/connection-reuse/
        /// </summary>
        public static async Task<IEnumerable<TResult>> ConcurrenceAsync<TResult, T>(IEnumerable<Task<T>> tasks, Func<T, TResult> convertor, bool continueOnCapturedContext = false)
        {
            List<TResult> results = new List<TResult>();

            foreach (Task<Task<T>> bucket in OrderByFinishedSequence(tasks))
            {
                Task<T> t = await bucket.ConfigureAwait(continueOnCapturedContext);

                T finishedT = await t.ConfigureAwait(continueOnCapturedContext);

                results.Add(convertor(finishedT));
            }

            return results;
        }

        /// <summary>
        /// 使用MySqlConnector的数据库操作，不能使用并行
        /// 详情请见 https://mysqlconnector.net/troubleshooting/connection-reuse/
        /// </summary>
        public static async Task<IEnumerable<TResult>> ConcurrenceAsync<TResult, T>(IEnumerable<Task<IEnumerable<T>>> tasks, Func<T, TResult> convertor, bool continueOnCapturedContext = false)
        {
            List<TResult> results = new List<TResult>();

            foreach (Task<Task<IEnumerable<T>>> bucket in OrderByFinishedSequence(tasks))
            {
                Task<IEnumerable<T>> t = await bucket.ConfigureAwait(continueOnCapturedContext);

                IEnumerable<T> finishedTs = await t.ConfigureAwait(continueOnCapturedContext);

                foreach (var item in finishedTs)
                {
                    results.Add(convertor(item));
                }
            }

            return results;
        }

        /// <summary>
        /// https://devblogs.microsoft.com/pfxteam/processing-tasks-as-they-complete/
        /// 按谁先执行完的顺序返回任务数组
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tasks"></param>
        /// <returns></returns>
        public static Task<Task<T>>[] OrderByFinishedSequence<T>(IEnumerable<Task<T>> tasks)
        {
            List<Task<T>> inputTasks = tasks.ToList();

            TaskCompletionSource<Task<T>>[] buckets = new TaskCompletionSource<Task<T>>[inputTasks.Count];
            Task<Task<T>>[] results = new Task<Task<T>>[buckets.Length];

            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new TaskCompletionSource<Task<T>>();
                results[i] = buckets[i].Task;
            }

            int nextTaskIndex = -1;

            foreach (Task<T> inputTask in inputTasks)
            {
                _ = inputTask.ContinueWith(continuation, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            }

            return results;

            void continuation(Task<T> completed)
            {
                TaskCompletionSource<Task<T>> bucket = buckets[Interlocked.Increment(ref nextTaskIndex)];
                bucket.TrySetResult(completed);
            }
        }
    }
}