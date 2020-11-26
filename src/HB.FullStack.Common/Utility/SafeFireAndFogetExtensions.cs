using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AsyncAwaitBestPractices;

namespace System.Threading.Tasks
{
    public static class SafeFireAndFogetExtensions
    {
        public static void Fire(this Task task)
        {
            task.SafeFireAndForget(GlobalSettings.ExceptionHandler, false);
        }

    }
}
