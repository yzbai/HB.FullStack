using System;
using System.Collections.Generic;
using System.Text;

namespace HB.Framework.Common.Threading
{
    public class FixedThreadPoolOptions
    {
        public int MinThreadNumber { get; set; } = 10;

        public int MaxThreadNumber { get; set; } = 1000;
    }

    public class FixedThreadPool
    {
        private FixedThreadPoolOptions _options;

        public FixedThreadPool(FixedThreadPoolOptions options)
        {
            _options = options;
        }

         
    }
}
