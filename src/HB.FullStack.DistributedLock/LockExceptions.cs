using System;
using System.Collections.Generic;

namespace HB.FullStack.Lock
{
    public static class LockExceptions
    {
        public static Exception MemoryLockError(string cause)
        {
            LockException exception = new LockException(ErrorCodes.MemoryLockError, cause);

            return exception;
        }

        public static Exception DistributedLockUnLockFailed(int threadId, IEnumerable<string> resources, Exception? innerException = null)
        {
            LockException exception = new LockException(ErrorCodes.DistributedLockUnLockFailed, nameof(DistributedLockUnLockFailed), innerException);

            exception.Data["ThreadId"] = threadId;
            exception.Data["Resources"] = resources;

            return exception;

        }
    }
}