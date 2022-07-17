﻿using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("HB.Infrastructure.Redis.DistributedLock")]
namespace HB.FullStack.Lock
{
    /// <summary>
    /// from 5000 - 5999
    /// </summary>
    internal static class LockErrorCodes
    {
        public static ErrorCode DistributedLockUnLockFailed { get; set; } = new ErrorCode(nameof(DistributedLockUnLockFailed), "");
        public static ErrorCode MemoryLockError { get; set; } = new ErrorCode(nameof(MemoryLockError), "");
    }

    internal static class LockExceptions
    {
        internal static Exception MemoryLockError(string cause)
        {
            LockException exception = new LockException(LockErrorCodes.MemoryLockError, cause);

            return exception;
        }

        internal static Exception DistributedLockUnLockFailed(int threadId, IEnumerable<string> resources, Exception? innerException = null)
        {
            LockException exception = new LockException(LockErrorCodes.DistributedLockUnLockFailed, nameof(DistributedLockUnLockFailed), innerException);

            exception.Data["ThreadId"] = threadId;
            exception.Data["Resources"] = resources;

            return exception;

        }
    }
}