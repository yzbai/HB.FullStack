using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace System.Threading.Tasks
{
    public static class TaskUtil
    {
        public static Task<IList<T>> FromList<T>()
        {
            IList<T> lst = new List<T>();

            return Task.FromResult(lst);
        }

    }
}
