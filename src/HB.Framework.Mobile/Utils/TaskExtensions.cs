using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;
using HB.Framework.Client.Base;

namespace System
{
    public static class TaskExtensions
    {
        public static void Fire(this Task task)
        {
            task.SafeFireAndForget(BaseApplication.ExceptionHandler);
        }
    }
}
