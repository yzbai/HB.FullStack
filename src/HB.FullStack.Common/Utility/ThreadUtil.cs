using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Threading;

namespace System
{
    public static class ThreadUtil
    {
        public static JoinableTaskFactory JoinableTaskFactory { get; } = new JoinableTaskFactory(new JoinableTaskContext());
    }
}
