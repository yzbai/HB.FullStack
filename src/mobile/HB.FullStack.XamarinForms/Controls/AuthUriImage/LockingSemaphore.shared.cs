using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HB.FullStack.XamarinForms.Controls
{
	internal class LockingSemaphore
	{
		static readonly Task _completed = Task.FromResult(true);
		readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
		int _currentCount;

		public LockingSemaphore(int initialCount)
		{
			if (initialCount < 0)
				throw new ArgumentOutOfRangeException(nameof(initialCount));
			_currentCount = initialCount;
		}

		public void Release()
		{
			TaskCompletionSource<bool>? toRelease = null;
			lock (_waiters)
			{
				if (_waiters.Count > 0)
					toRelease = _waiters.Dequeue();
				else
					++_currentCount;
			}
			if (toRelease != null)
				toRelease.TrySetResult(true);
		}

		public Task WaitAsync(CancellationToken token)
		{
			lock (_waiters)
			{
				if (_currentCount > 0)
				{
					--_currentCount;
					return _completed;
				}
				var waiter = new TaskCompletionSource<bool>();
				_waiters.Enqueue(waiter);
				token.Register(() => waiter.TrySetCanceled());
				return waiter.Task;
			}
		}
	}
}
