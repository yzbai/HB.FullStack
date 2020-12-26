using System;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using HB.FullStack.Client;
using HB.FullStack.Common;
using HB.FullStack.Common.Entities;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.Client.Repos
{
    public class BaseRepo<T> where T : Entity
    {
        private static bool _isDatabaseInitTaskNotWaitedYet = true;

        private readonly TimeSpan _localDataExpiryTime;

        protected static MemorySimpleLocker RequestLocker { get; } = new MemorySimpleLocker();

        public BaseRepo()
        {
            if (_isDatabaseInitTaskNotWaitedYet)
            {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
                Task.WaitAll(Application.Current.GetInitializeTaskAsync());
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

                _isDatabaseInitTaskNotWaitedYet = false;
            }

            var timeoutAttr = typeof(T).GetCustomAttribute<LocalDataTimeoutAttribute>();

            _localDataExpiryTime = timeoutAttr?.ExpiryTime ?? TimeSpan.FromMinutes(5);
        }

        protected bool LocalDataTimeout(string resourceType, string resource)
        {
            return RequestLocker.NoWaitLock(resourceType, resource, _localDataExpiryTime);
        }

        protected static void TimeoutLocalData(string resourceType, string resource)
        {
            RequestLocker.UnLock(resourceType, resource);
        }

        protected static void InsureLogined()
        {
            if (!ClientGlobal.IsLogined())
            {
                throw new ApiException(ErrorCode.ApiNoAuthority, System.Net.HttpStatusCode.Unauthorized);
            }
        }

        protected static void InsureInternet()
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                throw new ApiException(ErrorCode.ApiUnkown, System.Net.HttpStatusCode.BadGateway);
            }
        }
    }
}