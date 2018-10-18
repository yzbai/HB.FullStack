using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HB.Framework.Common
{
    //TODO: 考虑是否要用Polly库代替
    /// <summary>
    /// Retry the task without await. You can fire and forgot.
    /// </summary>
    public static class TaskRetryOldxx
    {
        private static Func<int, TimeSpan> defaultSleepDurationProvider = retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));

        private static Func<int, TimeSpan> zeroSleepDurationProvider = retryAttempt => TimeSpan.Zero;

        public static Func<int, TimeSpan> DefaultSleepDurationProvider { get => defaultSleepDurationProvider; set => defaultSleepDurationProvider = value; }
        public static Func<int, TimeSpan> ZeroSleepDurationProvider { get => zeroSleepDurationProvider; set => zeroSleepDurationProvider = value; }

        public static Task Retry(int retryCount, Func<Task> taskAction, Action<Task, AggregateException> exceptionAction, Func<int, TimeSpan> sleepDurationProvider = null)
        {
            if (retryCount < 0)
            {
                throw new ArgumentException("Retry Count must >= 0");
            }

            Task task = taskAction();

            if (retryCount == 1)
            {
                return task.ContinueWith(t => {
                    if (t.IsFaulted)
                    {
                        exceptionAction(t, t.Exception);
                    }

                    return t;
                }, TaskScheduler.Default);
            }

            if (sleepDurationProvider == null)
            {
                sleepDurationProvider = ZeroSleepDurationProvider;
            }

            for (int i = 1; i < retryCount; ++i)
            {
                task = task.ContinueWith<Task>(t => {
                    if (!t.IsFaulted)
                    {
                        return t;
                    }

                    exceptionAction(t, t.Exception);

                    Thread.Sleep(sleepDurationProvider(i));

                    return taskAction();
                }, TaskScheduler.Default);
            }

            return task;
        }

        public static Task<T> Retry<T>(int retryCount, Func<Task<T>> taskAction, Action<Task<T>, AggregateException> exceptionAction, Func<int, TimeSpan> sleepDurationProvider = null)
        {
            if (retryCount < 0)
            {
                throw new ArgumentException("Retry Count must >= 0");
            }

            Task<T> task = taskAction();

            if (retryCount == 1)
            {
                return task.ContinueWith<T>(t => {
                    if (t.IsFaulted)
                    {
                        exceptionAction(t, t.Exception);

                        return default(T);
                    }

                    return t.Result;
                }, TaskScheduler.Default);
            }

            if (sleepDurationProvider == null)
            {
                sleepDurationProvider = ZeroSleepDurationProvider;
            }

            for (int i = 1; i < retryCount; ++i)
            {
                task = task.ContinueWith<T>(t => {
                    if (!t.IsFaulted)
                    {
                        return t.Result;
                    }

                    exceptionAction(t, t.Exception);

                    Thread.Sleep(sleepDurationProvider(i));

                    return taskAction().Result;
                }, TaskScheduler.Default);
            }

            return task;
        }
    }
}
