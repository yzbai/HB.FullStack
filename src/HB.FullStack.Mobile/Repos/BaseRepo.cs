using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Threading.Tasks;
using HB.FullStack.Common;
using HB.FullStack.Common.Api;
using HB.FullStack.Common.Entities;
using HB.FullStack.Common.Resources;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace HB.FullStack.Client.Repos
{
    public class BaseRepo<T> where T : Entity
    {
        private static bool _isDatabaseInitTaskNotWaitedYet = true;

        private readonly TimeSpan? _localDataExpiryTime;

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

            _localDataExpiryTime = timeoutAttr?.ExpiryTime;
        }

        protected bool LocalDataAvailable<TRes>(ApiRequest<TRes> apiRequest) where TRes : Resource
        {
            if (_localDataExpiryTime.HasValue)
            {
                return RequestLocker.NoWaitLock(apiRequest.GetType().FullName, apiRequest.GetHashCode().ToString(CultureInfo.InvariantCulture), _localDataExpiryTime.Value);
            }

            return false;
        }

        protected static void TimeoutLocalData<TRes>(ApiRequest<TRes> apiRequest) where TRes : Resource
        {
            RequestLocker.UnLock(apiRequest.GetType().FullName, apiRequest.GetHashCode().ToString(CultureInfo.InvariantCulture));
        }

        protected static void InsureLogined()
        {
            if (UserPreferences.IsLogined())
            {
                throw new ApiException(ErrorCode.ApiNoAuthority, System.Net.HttpStatusCode.Unauthorized);
            }
        }

        protected static bool InsureInternet(bool throwOnNoInternet = true)
        {
            if (Connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                if (throwOnNoInternet)
                {
                    throw new ApiException(ErrorCode.ApiUnkown, System.Net.HttpStatusCode.BadGateway);
                }
                return false;
            }

            return true;
        }


    }
}